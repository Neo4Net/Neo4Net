using System;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Kernel.monitoring.tracing
{
	using Service = Neo4Net.Helpers.Service;
	using PageCacheTracer = Neo4Net.Io.pagecache.tracing.PageCacheTracer;
	using DefaultPageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.DefaultPageCursorTracerSupplier;
	using PageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using CheckPointTracer = Neo4Net.Kernel.impl.transaction.tracing.CheckPointTracer;
	using TransactionTracer = Neo4Net.Kernel.impl.transaction.tracing.TransactionTracer;
	using Log = Neo4Net.Logging.Log;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using LockTracer = Neo4Net.Kernel.Api.StorageEngine.@lock.LockTracer;
	using SystemNanoClock = Neo4Net.Time.SystemNanoClock;

	/// <summary>
	/// <h1>Tracers</h1>
	/// <para>
	///     While monitoring is a dynamic piece of infrastructure, where monitors can be wired up and torn down on the fly,
	///     the tracing infrastructure is static and hard-coded into the database on startup.
	/// </para>
	/// <para>
	///     Tracing is always on, though the default implementation is very light weight, incurring almost no overhead.
	/// </para>
	/// <h2>The Tracers class</h2>
	/// <para>
	///     This is the central hub in the tracing infrastructure.
	/// </para>
	/// <para>
	///     This class is responsible for choosing what tracing implementation to use, and for creating the relevant tracer
	///     components to distribute throughout the database instance.
	/// </para>
	/// <para>
	///     The tracing implementation is determined by the {@code unsupported.dbms.tracer} setting. Two built-in implementations
	///     exist: {@code default} and {@code null}. Alternative implementations can be loaded from the
	///     classpath by referencing their <seealso cref="Neo4Net.kernel.monitoring.tracing.TracerFactory"/> in a
	///     {@code META-INF/services/Neo4Net.kernel.monitoring.tracing.TracerFactory}, and setting
	///     {@code unsupported.dbms.tracer} to the appropriate value.
	/// </para>
	/// <h2>Designing and implementing tracers</h2>
	/// <para>
	///     There are two parts to tracers: the tracer implementation, which starts with the TracerFactory, and the
	///     subsystems that expose themselves to tracing.
	/// </para>
	/// <para>
	///     The traced subsystems are responsible for defining their own tracer and trace event interfaces, and they are
	///     responsible for getting hold of a tracer implementation through the Tracers class, and for producing trace
	///     events and feeding them with data.
	/// </para>
	/// <para>
	///     Traced subsystems define a hierarchy of events: large coarse grain events can contain smaller and more
	///     detailed events. Sibling events (that follow one after another in time) are typically spawned from the same
	///     parent event. The tracers and trace events are all defined as interfaces, and each interface should have a
	///     {@code NULL} field that references an implementation that does nothing, other than return other {@code NULL}
	///     implementations of any child event interfaces. The existing trace interfaces for transactions and the page
	///     cache, are good examples of this.
	/// </para>
	/// <para>
	///     The tracer implementations are responsible for implementing all the tracer and trace event interfaces in a way
	///     that is both fast, and robust. Robustness is important because tracer implementations are not allowed to throw
	///     exceptions, and they are not allowed to return {@code null} where a trace event is expected. Implementations
	///     may add implementation specific data to the events, if they want to report implementation specific data.
	///     They are also allowed to produce the {@code NULL} implementations that are associated with the various tracer
	///     and trace event interfaces. If, for instance, the implementation is not interested in the data that would be
	///     collected from a given trace event, then it can choose to use the {@code NULL} implementation. It could also
	///     be that something went wrong when building an event instance of the desired type, and since it cannot return
	///     {@code null} or throw exceptions, it is forced to return the {@code NULL} implementation.
	/// </para>
	/// <para>
	///     Tracer implementations should prefer to always return the same trace event implementation type for a given
	///     trace event type. Using more than one implementation type impairs JIT optimisation, as it causes the callsites
	///     in the traced code to no longer be monomorphic. Implementations should be built with performance in mind, as
	///     the code being traced is often quite important for the performance of the database.
	/// </para>
	/// <para>
	///     The {@code default} and {@code null} implementation are always available, and 3rd party implementations can
	///     piggy-back on them and extend them. At least one 3rd party implementation is known at this point; the
	///     <a href="https://github.com/Neo4Net-contrib/Neo4Net-jfr">Neo4Net-jfr implementation</a>. It is recommended that
	///     those change the tracer or trace event interfaces, or add tracing to more subsystems, also make sure to keep
	///     the Neo4Net-jfr code base up to date.
	/// </para>
	/// </summary>
	public class Tracers
	{
		 public readonly PageCacheTracer PageCacheTracer;
		 public readonly PageCursorTracerSupplier PageCursorTracerSupplier;
		 public readonly TransactionTracer TransactionTracer;
		 public readonly CheckPointTracer CheckPointTracer;
		 public readonly LockTracer LockTracer;

		 /// <summary>
		 /// Create a Tracers subsystem with the desired implementation, if it can be found and created.
		 /// 
		 /// Otherwise the default implementation is used, and a warning is logged to the given StringLogger. </summary>
		 /// <param name="desiredImplementationName"> The name of the desired {@link Neo4Net.kernel.monitoring.tracing
		 /// .TracerFactory} implementation, as given by its <seealso cref="TracerFactory.getImplementationName()"/> method. </param>
		 /// <param name="msgLog"> A <seealso cref="Log"/> for logging when the desired implementation cannot be created. </param>
		 /// <param name="monitors"> the monitoring manager </param>
		 /// <param name="jobScheduler"> a scheduler for async jobs </param>
		 public Tracers( string desiredImplementationName, Log msgLog, Monitors monitors, IJobScheduler jobScheduler, SystemNanoClock clock )
		 {
			  if ( "null".Equals( desiredImplementationName, StringComparison.OrdinalIgnoreCase ) )
			  {
					PageCursorTracerSupplier = DefaultPageCursorTracerSupplier.NULL;
					PageCacheTracer = PageCacheTracer.NULL;
					TransactionTracer = Neo4Net.Kernel.impl.transaction.tracing.TransactionTracer_Fields.Null;
					CheckPointTracer = Neo4Net.Kernel.impl.transaction.tracing.CheckPointTracer_Fields.Null;
					LockTracer = LockTracer.NONE;
			  }
			  else
			  {
					TracerFactory foundFactory = new DefaultTracerFactory();
					bool found = string.ReferenceEquals( desiredImplementationName, null );
					foreach ( TracerFactory factory in Service.load( typeof( TracerFactory ) ) )
					{
						 try
						 {
							  if ( factory.ImplementationName.Equals( desiredImplementationName, StringComparison.OrdinalIgnoreCase ) )
							  {
									foundFactory = factory;
									found = true;
									break;
							  }
						 }
						 catch ( Exception e )
						 {
							  msgLog.Warn( "Failed to instantiate desired tracer implementations '" + desiredImplementationName + "'", e );
						 }
					}

					if ( !found )
					{
						 msgLog.Warn( "Using default tracer implementations instead of '%s'", desiredImplementationName );
					}

					PageCursorTracerSupplier = foundFactory.CreatePageCursorTracerSupplier( monitors, jobScheduler );
					PageCacheTracer = foundFactory.CreatePageCacheTracer( monitors, jobScheduler, clock, msgLog );
					TransactionTracer = foundFactory.CreateTransactionTracer( monitors, jobScheduler );
					CheckPointTracer = foundFactory.CreateCheckPointTracer( monitors, jobScheduler );
					LockTracer = foundFactory.CreateLockTracer( monitors, jobScheduler );
			  }
		 }
	}

}