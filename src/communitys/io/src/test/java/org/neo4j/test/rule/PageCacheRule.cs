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
namespace Neo4Net.Test.rule
{
	using ObjectUtils = org.apache.commons.lang3.ObjectUtils;


	using Adversary = Neo4Net.Adversaries.Adversary;
	using AdversarialPageCache = Neo4Net.Adversaries.pagecache.AdversarialPageCache;
	using Configuration = Neo4Net.Graphdb.config.Configuration;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using MemoryAllocator = Neo4Net.Io.mem.MemoryAllocator;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using AccessCheckingPageCache = Neo4Net.Io.pagecache.checking.AccessCheckingPageCache;
	using SingleFilePageSwapperFactory = Neo4Net.Io.pagecache.impl.SingleFilePageSwapperFactory;
	using MuninnPageCache = Neo4Net.Io.pagecache.impl.muninn.MuninnPageCache;
	using PageCacheTracer = Neo4Net.Io.pagecache.tracing.PageCacheTracer;
	using PageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using VersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.VersionContextSupplier;
	using LocalMemoryTracker = Neo4Net.Memory.LocalMemoryTracker;
	using JobScheduler = Neo4Net.Scheduler.JobScheduler;
	using ThreadPoolJobScheduler = Neo4Net.Scheduler.ThreadPoolJobScheduler;

	public class PageCacheRule : ExternalResource
	{
		 /// <summary>
		 /// Class to alter behavior and configuration of <seealso cref="PageCache"/> instances opened in this rule.
		 /// </summary>
		 public sealed class PageCacheConfig
		 {
			  protected internal bool? InconsistentReads;
			  protected internal int? PageSize;
			  protected internal AtomicBoolean NextReadIsInconsistent;
			  protected internal PageCacheTracer Tracer;
			  protected internal PageCursorTracerSupplier PageCursorTracerSupplier;
			  internal bool AccessChecks;
			  internal string Memory;

			  internal PageCacheConfig()
			  {
			  }

			  /// <summary>
			  /// Sets whether or not to decorate PageCache where the read page cursors will randomly produce inconsistent
			  /// reads with a ~50% probability.
			  /// </summary>
			  /// <param name="inconsistentReads"> {@code true} if PageCache should be decorated with read cursors with
			  /// randomly inconsistent reads. </param>
			  /// <returns> this instance. </returns>
			  public PageCacheConfig WithInconsistentReads( bool inconsistentReads )
			  {
					this.InconsistentReads = inconsistentReads;
					return this;
			  }

			  /// <summary>
			  /// Decorated PageCache where the next page read from a read page cursor will be
			  /// inconsistent if the given AtomicBoolean is set to 'true'. The AtomicBoolean is automatically
			  /// switched to 'false' when the inconsistent read is performed, to prevent code from looping
			  /// forever.
			  /// </summary>
			  /// <param name="nextReadIsInconsistent"> an <seealso cref="AtomicBoolean"/> for controlling when inconsistent reads happen. </param>
			  /// <returns> this instance. </returns>
			  public PageCacheConfig WithInconsistentReads( AtomicBoolean nextReadIsInconsistent )
			  {
					this.NextReadIsInconsistent = nextReadIsInconsistent;
					this.InconsistentReads = true;
					return this;
			  }

			  /// <summary>
			  /// Makes PageCache have the specified page size.
			  /// </summary>
			  /// <param name="pageSize"> page size to use instead of hinted page size. </param>
			  /// <returns> this instance. </returns>
			  public PageCacheConfig WithPageSize( int pageSize )
			  {
					this.PageSize = pageSize;
					return this;
			  }

			  /// <summary>
			  /// <seealso cref="PageCacheTracer"/> to use for the PageCache.
			  /// </summary>
			  /// <param name="tracer"> <seealso cref="PageCacheTracer"/> to use. </param>
			  /// <returns> this instance. </returns>
			  public PageCacheConfig WithTracer( PageCacheTracer tracer )
			  {
					this.Tracer = tracer;
					return this;
			  }

			  /// <summary>
			  /// <seealso cref="PageCursorTracerSupplier"/> to use for this page cache. </summary>
			  /// <param name="tracerSupplier"> supplier of page cursors tracers </param>
			  /// <returns> this instance </returns>
			  public PageCacheConfig WithCursorTracerSupplier( PageCursorTracerSupplier tracerSupplier )
			  {
					this.PageCursorTracerSupplier = tracerSupplier;
					return this;
			  }

			  /// <summary>
			  /// Decorates PageCache with access checking wrapper to add some amount of verifications that
			  /// reads happen inside shouldRetry-loops.
			  /// </summary>
			  /// <param name="accessChecks"> whether or not to add access checking to the opened PageCache. </param>
			  /// <returns> this instance. </returns>
			  public PageCacheConfig WithAccessChecks( bool accessChecks )
			  {
					this.AccessChecks = accessChecks;
					return this;
			  }

			  /// <summary>
			  /// Overrides default memory setting, which is a standard test size of '8 MiB'.
			  /// </summary>
			  /// <param name="memory"> memory setting to use for this page cache. </param>
			  /// <returns> this instance. </returns>
			  public PageCacheConfig WithMemory( string memory )
			  {
					this.Memory = memory;
					return this;
			  }
		 }

		 /// <returns> new <seealso cref="PageCacheConfig"/> instance. </returns>
		 public static PageCacheConfig Config()
		 {
			  return new PageCacheConfig();
		 }

		 protected internal JobScheduler JobScheduler;
		 protected internal PageCache PageCache;
		 internal readonly PageCacheConfig BaseConfig;

		 public PageCacheRule() : this(Config())
		 {
		 }

		 public PageCacheRule( PageCacheConfig config )
		 {
			  this.BaseConfig = config;
		 }

		 public virtual PageCache GetPageCache( FileSystemAbstraction fs )
		 {
			  return GetPageCache( fs, Config() );
		 }

		 /// <summary>
		 /// Opens a new <seealso cref="PageCache"/> with the provided file system and config.
		 /// </summary>
		 /// <param name="fs"> <seealso cref="FileSystemAbstraction"/> to use for the <seealso cref="PageCache"/>. </param>
		 /// <param name="overriddenConfig"> specific <seealso cref="PageCacheConfig"/> overriding config provided in <seealso cref="PageCacheRule"/>
		 /// constructor, if any. </param>
		 /// <returns> the opened <seealso cref="PageCache"/>. </returns>
		 public virtual PageCache GetPageCache( FileSystemAbstraction fs, PageCacheConfig overriddenConfig )
		 {
			  CloseExistingPageCache();
			  int? pageSize = SelectConfig( BaseConfig.pageSize, overriddenConfig.PageSize, null );
			  PageCacheTracer cacheTracer = SelectConfig( BaseConfig.tracer, overriddenConfig.Tracer, PageCacheTracer.NULL );
			  PageCursorTracerSupplier cursorTracerSupplier = SelectConfig( BaseConfig.pageCursorTracerSupplier, overriddenConfig.PageCursorTracerSupplier, Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null );

			  SingleFilePageSwapperFactory factory = new SingleFilePageSwapperFactory();
			  factory.Open( fs, Configuration.EMPTY );
			  VersionContextSupplier contextSupplier = EmptyVersionContextSupplier.EMPTY;
			  MemoryAllocator mman = MemoryAllocator.createAllocator( SelectConfig( BaseConfig.memory, overriddenConfig.Memory, "8 MiB" ), new LocalMemoryTracker() );
			  InitializeJobScheduler();
			  if ( pageSize != null )
			  {
					PageCache = new MuninnPageCache( factory, mman, pageSize.Value, cacheTracer, cursorTracerSupplier, contextSupplier, JobScheduler );
			  }
			  else
			  {
					PageCache = new MuninnPageCache( factory, mman, cacheTracer, cursorTracerSupplier, contextSupplier, JobScheduler );
			  }
			  PageCachePostConstruct( overriddenConfig );
			  return PageCache;
		 }

		 protected internal virtual void InitializeJobScheduler()
		 {
			  JobScheduler = new ThreadPoolJobScheduler();
		 }

		 protected internal static T SelectConfig<T>( T @base, T overridden, T defaultValue )
		 {
			  return ObjectUtils.firstNonNull( @base, overridden, defaultValue );
		 }

		 protected internal virtual void PageCachePostConstruct( PageCacheConfig overriddenConfig )
		 {
			  if ( SelectConfig( BaseConfig.inconsistentReads, overriddenConfig.InconsistentReads, true ) )
			  {
					AtomicBoolean controller = SelectConfig( BaseConfig.nextReadIsInconsistent, overriddenConfig.NextReadIsInconsistent, null );
					Adversary adversary = controller != null ? new AtomicBooleanInconsistentReadAdversary( controller ) : new RandomInconsistentReadAdversary();
					PageCache = new AdversarialPageCache( PageCache, adversary );
			  }
			  if ( SelectConfig( BaseConfig.accessChecks, overriddenConfig.AccessChecks, false ) )
			  {
					PageCache = new AccessCheckingPageCache( PageCache );
			  }
		 }

		 protected internal virtual void CloseExistingPageCache()
		 {
			  ClosePageCache( "Failed to stop existing PageCache prior to creating a new one." );
			  CloseJobScheduler( "Failed to stop existing job scheduler prior to creating a new one." );
		 }

		 protected internal override void After( bool success )
		 {
			  ClosePageCache( "Failed to stop PageCache after test." );
			  CloseJobScheduler( "Failed to stop job scheduler after test." );
		 }

		 private void CloseJobScheduler( string errorMessage )
		 {
			  if ( JobScheduler != null )
			  {
					try
					{
						 JobScheduler.close();
					}
					catch ( Exception e )
					{
						 throw new Exception( errorMessage, e );
					}
					JobScheduler = null;
			  }
		 }

		 private void ClosePageCache( string errorMessage )
		 {
			  if ( PageCache != null )
			  {
					try
					{
						 PageCache.close();
					}
					catch ( Exception e )
					{
						 throw new AssertionError( errorMessage, e );
					}
					PageCache = null;
			  }
		 }

		 public class AtomicBooleanInconsistentReadAdversary : Adversary
		 {
			  internal readonly AtomicBoolean NextReadIsInconsistent;

			  public AtomicBooleanInconsistentReadAdversary( AtomicBoolean nextReadIsInconsistent )
			  {
					this.NextReadIsInconsistent = nextReadIsInconsistent;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SafeVarargs public final void injectFailure(Class... failureTypes)
			  public override void InjectFailure( params Type[] failureTypes )
			  {
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SafeVarargs public final boolean injectFailureOrMischief(Class... failureTypes)
			  public override bool InjectFailureOrMischief( params Type[] failureTypes )
			  {
					return NextReadIsInconsistent.getAndSet( false );
			  }
		 }

		 private class RandomInconsistentReadAdversary : Adversary
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SafeVarargs public final void injectFailure(Class... failureTypes)
			  public override void InjectFailure( params Type[] failureTypes )
			  {
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SafeVarargs public final boolean injectFailureOrMischief(Class... failureTypes)
			  public override bool InjectFailureOrMischief( params Type[] failureTypes )
			  {
					return ThreadLocalRandom.current().nextBoolean();
			  }
		 }
	}

}