/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.causalclustering.helper
{
	using Lifecycle = Org.Neo4j.Kernel.Lifecycle.Lifecycle;
	using Log = Org.Neo4j.Logging.Log;

	public abstract class SuspendableLifeCycle : Lifecycle, Suspendable
	{
		 private readonly Log _debugLog;
		 private bool _stoppedByLifeCycle = true;
		 private bool _enabled = true;

		 public SuspendableLifeCycle( Log debugLog )
		 {
			  this._debugLog = debugLog;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final synchronized void enable() throws Throwable
		 public override void Enable()
		 {
			 lock ( this )
			 {
				  if ( !_stoppedByLifeCycle )
				  {
						Start0();
				  }
				  else
				  {
						_debugLog.info( "%s will not start. It was enabled but is stopped by lifecycle", this );
				  }
				  _enabled = true;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final synchronized void disable() throws Throwable
		 public override void Disable()
		 {
			 lock ( this )
			 {
				  Stop0();
				  _enabled = false;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final synchronized void init() throws Throwable
		 public override void Init()
		 {
			 lock ( this )
			 {
				  Init0();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final synchronized void start() throws Throwable
		 public override void Start()
		 {
			 lock ( this )
			 {
				  if ( !_enabled )
				  {
						_debugLog.info( "Start call from lifecycle is ignored because %s is disabled.", this );
				  }
				  else
				  {
						Start0();
				  }
				  _stoppedByLifeCycle = false;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final synchronized void stop() throws Throwable
		 public override void Stop()
		 {
			 lock ( this )
			 {
				  Stop0();
				  _stoppedByLifeCycle = true;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public final synchronized void shutdown() throws Throwable
		 public override void Shutdown()
		 {
			 lock ( this )
			 {
				  Shutdown0();
				  _stoppedByLifeCycle = true;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract void init0() throws Throwable;
		 protected internal abstract void Init0();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract void start0() throws Throwable;
		 protected internal abstract void Start0();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract void stop0() throws Throwable;
		 protected internal abstract void Stop0();

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected abstract void shutdown0() throws Throwable;
		 protected internal abstract void Shutdown0();
	}

}