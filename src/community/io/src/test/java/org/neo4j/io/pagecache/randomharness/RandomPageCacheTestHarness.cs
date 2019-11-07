using System;
using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.Io.pagecache.randomharness
{

	using RandomAdversary = Neo4Net.Adversaries.RandomAdversary;
	using AdversarialFileSystemAbstraction = Neo4Net.Adversaries.fs.AdversarialFileSystemAbstraction;
	using Configuration = Neo4Net.GraphDb.config.Configuration;
	using EphemeralFileSystemAbstraction = Neo4Net.GraphDb.mockfs.EphemeralFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using SingleFilePageSwapperFactory = Neo4Net.Io.pagecache.impl.SingleFilePageSwapperFactory;
	using MuninnPageCache = Neo4Net.Io.pagecache.impl.muninn.MuninnPageCache;
	using PageCacheTracer = Neo4Net.Io.pagecache.tracing.PageCacheTracer;
	using PageCursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier;
	using EmptyVersionContextSupplier = Neo4Net.Io.pagecache.tracing.cursor.context.EmptyVersionContextSupplier;
	using LinearHistoryPageCacheTracerTest = Neo4Net.Io.pagecache.tracing.linear.LinearHistoryPageCacheTracerTest;
	using LinearTracers = Neo4Net.Io.pagecache.tracing.linear.LinearTracers;
	using Profiler = Neo4Net.Resources.Profiler;
	using DaemonThreadFactory = Neo4Net.Scheduler.DaemonThreadFactory;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using ThreadPoolJobScheduler = Neo4Net.Scheduler.ThreadPoolJobScheduler;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

	/// <summary>
	/// The RandomPageCacheTestHarness can plan and run random page cache tests, repeatably if necessary, and verify that
	/// the behaviour of the page cache is correct to some degree. For instance, it can verify that records don't end up
	/// overlapping each other in the mapped files, that records end up at the correct locations in the files, and that
	/// records don't end up in the wrong files. The harness can also execute separate preparation and verification steps,
	/// before and after executing the planned test respectively, and it can integrate with the adversarial file system
	/// for fault injection, and arbitrary PageCacheTracers.
	/// <para>
	/// See <seealso cref="LinearHistoryPageCacheTracerTest"/> for an example of how to configure and use the harness.
	/// </para>
	/// </summary>
	public class RandomPageCacheTestHarness : System.IDisposable
	{
		 private static readonly ExecutorService _executorService = new ThreadPoolExecutor( 0, int.MaxValue, 1, TimeUnit.SECONDS, new SynchronousQueue<>(), new DaemonThreadFactory() );

		 private double _mischiefRate;
		 private double _failureRate;
		 private double _errorRate;
		 private int _concurrencyLevel;
		 private int _initialMappedFiles;
		 private int _cachePageCount;
		 private int _filePageCount;
		 private int _filePageSize;
		 private PageCacheTracer _tracer;
		 private PageCursorTracerSupplier _cursorTracerSupplier;
		 private int _commandCount;
		 private double[] _commandProbabilityFactors;
		 private long _randomSeed;
		 private bool _fixedRandomSeed;
		 private FileSystemAbstraction _fs;
		 private bool _useAdversarialIO;
		 private Plan _plan;
		 private Phase _preparation;
		 private Phase _verification;
		 private RecordFormat _recordFormat;
		 private Profiler _profiler;

		 public RandomPageCacheTestHarness()
		 {
			  _mischiefRate = 0.1;
			  _failureRate = 0.1;
			  _errorRate = 0.0;
			  _concurrencyLevel = 1;
			  _initialMappedFiles = 2;
			  _cachePageCount = 20;
			  _filePageCount = _cachePageCount * 10;
			  _tracer = PageCacheTracer.NULL;
			  _cursorTracerSupplier = Neo4Net.Io.pagecache.tracing.cursor.PageCursorTracerSupplier_Fields.Null;
			  _commandCount = 1000;

			  Command[] commands = Command.values();
			  _commandProbabilityFactors = new double[commands.Length];
			  foreach ( Command command in commands )
			  {
					_commandProbabilityFactors[command.ordinal()] = command.DefaultProbabilityFactor;
			  }

			  _fs = new EphemeralFileSystemAbstraction();
			  _useAdversarialIO = true;
			  _recordFormat = new StandardRecordFormat();
			  _profiler = Profiler.nullProfiler();
		 }

		 /// <summary>
		 /// Disable all of the given commands, by setting their probability factors to zero.
		 /// </summary>
		 public virtual void DisableCommands( params Command[] commands )
		 {
			  foreach ( Command command in commands )
			  {
					SetCommandProbabilityFactor( command, 0 );
			  }
		 }

		 /// <summary>
		 /// Set the probability factor of the given command. The default value is given by
		 /// <seealso cref="Command.getDefaultProbabilityFactor()"/>. The effective probability is computed from the relative
		 /// difference in probability factors between all the commands.
		 /// <para>
		 /// Setting the probability factor to zero will disable that command.
		 /// </para>
		 /// </summary>
		 public virtual void SetCommandProbabilityFactor( Command command, double probabilityFactor )
		 {
			  Debug.Assert( 0.0 <= probabilityFactor, "Probability factor cannot be negative" );
			  _commandProbabilityFactors[command.ordinal()] = probabilityFactor;
		 }

		 /// <summary>
		 /// Set to "true" to execute the plans with fault injection from the <seealso cref="AdversarialFileSystemAbstraction"/>, or
		 /// set to "false" to disable this feature.
		 /// <para>
		 /// The default is "true".
		 /// </para>
		 /// </summary>
		 public virtual bool UseAdversarialIO
		 {
			 set
			 {
				  this._useAdversarialIO = value;
			 }
		 }

		 /// <summary>
		 /// Set the PageCacheTracer that the page cache under test should be configured with.
		 /// </summary>
		 public virtual PageCacheTracer Tracer
		 {
			 set
			 {
				  this._tracer = value;
			 }
		 }

		 /// <summary>
		 /// Set the page cursor tracers supplier.
		 /// </summary>
		 public virtual PageCursorTracerSupplier CursorTracerSupplier
		 {
			 set
			 {
				  this._cursorTracerSupplier = value;
			 }
		 }

		 /// <summary>
		 /// Set the mischief rate for the adversarial file system.
		 /// </summary>
		 public virtual double MischiefRate
		 {
			 set
			 {
				  _mischiefRate = value;
			 }
		 }

		 /// <summary>
		 /// Set the failure rate for the adversarial file system.
		 /// </summary>
		 public virtual double FailureRate
		 {
			 set
			 {
				  _failureRate = value;
			 }
		 }

		 /// <summary>
		 /// Set the error rate for the adversarial file system.
		 /// </summary>
		 public virtual double ErrorRate
		 {
			 set
			 {
				  _errorRate = value;
			 }
		 }

		 /// <summary>
		 /// Set the number of threads that will execute commands from the plan. If this number is greater than 1, then the
		 /// plan will execute non-deterministically. The description of the iteration that
		 /// <seealso cref="describePreviousRun(PrintStream)"/> prints will include which thread performed which command.
		 /// </summary>
		 public virtual int ConcurrencyLevel
		 {
			 set
			 {
				  this._concurrencyLevel = value;
			 }
		 }

		 /// <summary>
		 /// Set the number of files that should be mapped from the start of the plan. If you have set the probability of
		 /// the <seealso cref="Command.MapFile"/> command to zero, then you must have a positive number of initial mapped files.
		 /// Otherwise there will be no files to plan any work for.
		 /// <para>
		 /// The default value is 2.
		 /// </para>
		 /// </summary>
		 public virtual int InitialMappedFiles
		 {
			 set
			 {
				  this._initialMappedFiles = value;
			 }
		 }

		 public virtual int CachePageCount
		 {
			 set
			 {
				  this._cachePageCount = value;
			 }
		 }

		 public virtual int FilePageCount
		 {
			 set
			 {
				  this._filePageCount = value;
			 }
		 }

		 public virtual int FilePageSize
		 {
			 set
			 {
				  this._filePageSize = value;
			 }
		 }

		 /// <summary>
		 /// Set the number of commands to plan in each iteration.
		 /// </summary>
		 public virtual int CommandCount
		 {
			 set
			 {
				  this._commandCount = value;
			 }
		 }

		 /// <summary>
		 /// Set the preparation phase to use. This phase is executed before all the planned commands. It can be used to
		 /// prepare some file contents, or reset some external state, such as the
		 /// <seealso cref="LinearTracers"/>.
		 /// <para>
		 /// The preparation phase is executed before each iteration.
		 /// </para>
		 /// </summary>
		 public virtual Phase Preparation
		 {
			 set
			 {
				  this._preparation = value;
			 }
		 }

		 /// <summary>
		 /// Set the verification phase to use. This phase is executed after all the planned commands have executed
		 /// completely, and can be used to verify the consistency of the data, or some other invariant.
		 /// <para>
		 /// The verification phase is executed after each iteration.
		 /// </para>
		 /// </summary>
		 public virtual Phase Verification
		 {
			 set
			 {
				  this._verification = value;
			 }
		 }

		 /// <summary>
		 /// Set the record format to use. The record format is used to read, write and verify file contents.
		 /// </summary>
		 public virtual RecordFormat RecordFormat
		 {
			 set
			 {
				  this._recordFormat = value;
			 }
		 }

		 /// <summary>
		 /// Set and fix the random seed to the given value. All iterations run through this harness will then use that seed.
		 /// <para>
		 /// If the random seed has not been configured, then each iteration will use a new seed.
		 /// </para>
		 /// </summary>
		 public virtual long RandomSeed
		 {
			 set
			 {
				  this._randomSeed = value;
				  this._fixedRandomSeed = true;
			 }
		 }

		 public virtual FileSystemAbstraction FileSystem
		 {
			 set
			 {
				  this._fs = value;
			 }
		 }

		 public virtual void UseProfiler( Profiler profiler )
		 {
			  this._profiler = profiler;
		 }

		 /// <summary>
		 /// Write out a textual description of the last run iteration, including the exact plan and what thread
		 /// executed which command, and the random seed that can be used to recreate that plan for improved repeatability.
		 /// </summary>
		 public virtual void DescribePreviousRun( PrintStream @out )
		 {
			  @out.println( "randomSeed = " + _randomSeed );
			  @out.println( "commandCount = " + _commandCount );
			  @out.println( "concurrencyLevel (number of worker threads) = " + _concurrencyLevel );
			  @out.println( "initialMappedFiles = " + _initialMappedFiles );
			  @out.println( "cachePageCount = " + _cachePageCount );
			  @out.println( "tracer = " + _tracer );
			  @out.println( "useAdversarialIO = " + _useAdversarialIO );
			  @out.println( "mischeifRate = " + _mischiefRate );
			  @out.println( "failureRate = " + _failureRate );
			  @out.println( "errorRate = " + _errorRate );
			  @out.println( "Command probability factors:" );
			  Command[] commands = Command.values();
			  for ( int i = 0; i < commands.Length; i++ )
			  {
					@out.print( "  " );
					@out.print( commands[i] );
					@out.print( " = " );
					@out.println( _commandProbabilityFactors[i] );
			  }
			  if ( _plan != null )
			  {
					_plan.print( @out );
			  }
		 }

		 /// <summary>
		 /// Run a single iteration with the current harness configuration.
		 /// <para>
		 /// This will either complete within the given timeout, or throw an exception.
		 /// </para>
		 /// <para>
		 /// If the run fails, then a description will be printed to System.err.
		 /// </para>
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void run(long iterationTimeout, java.util.concurrent.TimeUnit unit) throws Exception
		 public virtual void Run( long iterationTimeout, TimeUnit unit )
		 {
			  Run( 1, iterationTimeout, unit );
		 }

		 /// <summary>
		 /// Run the given number of iterations with the given harness configuration.
		 /// <para>
		 /// If the random seed has been set to a specific value, then all iterations will use that seed. Otherwise each
		 /// iteration will use a new seed.
		 /// </para>
		 /// <para>
		 /// The given timeout applies to the individual iteration, not to their combined run. This is effectively similar
		 /// to calling <seealso cref="run(long, TimeUnit)"/> the given number of times.
		 /// </para>
		 /// <para>
		 /// The run will stop at the first failure, if any, and print a description of it to System.err.
		 /// </para>
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void run(int iterations, long iterationTimeout, java.util.concurrent.TimeUnit unit) throws Exception
		 public virtual void Run( int iterations, long iterationTimeout, TimeUnit unit )
		 {
			  try
			  {
					for ( int i = 0; i < iterations; i++ )
					{
						 RunIteration( iterationTimeout, unit );
					}
			  }
			  catch ( Exception e )
			  {
					DescribePreviousRun( System.err );
					throw e;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _fs.Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private void runIteration(long timeout, java.util.concurrent.TimeUnit unit) throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 private void RunIteration( long timeout, TimeUnit unit )
		 {
			  Debug.Assert( _filePageSize % _recordFormat.RecordSize == 0, "File page size must be a multiple of the record size" );

			  if ( !_fixedRandomSeed )
			  {
					_randomSeed = ThreadLocalRandom.current().nextLong();
			  }

			  FileSystemAbstraction fs = this._fs;
			  File[] files = BuildFileNames();

			  RandomAdversary adversary = new RandomAdversary( _mischiefRate, _failureRate, _errorRate );
			  adversary.ProbabilityFactor = 0.0;
			  if ( _useAdversarialIO )
			  {
					adversary.Seed = _randomSeed;
					fs = new AdversarialFileSystemAbstraction( adversary, fs );
			  }

			  PageSwapperFactory swapperFactory = new SingleFilePageSwapperFactory();
			  swapperFactory.Open( fs, Configuration.EMPTY );
			  IJobScheduler jobScheduler = new ThreadPoolJobScheduler();
			  MuninnPageCache cache = new MuninnPageCache( swapperFactory, _cachePageCount, _tracer, _cursorTracerSupplier, EmptyVersionContextSupplier.EMPTY, jobScheduler );
			  if ( _filePageSize == 0 )
			  {
					_filePageSize = cache.PageSize();
			  }
			  cache.PrintExceptionsOnClose = false;
			  IDictionary<File, PagedFile> fileMap = new Dictionary<File, PagedFile>( Files.Length );
			  for ( int i = 0; i < Math.Min( Files.Length, _initialMappedFiles ); i++ )
			  {
					File file = files[i];
					fileMap[file] = cache.Map( file, _filePageSize );
			  }

			  _plan = _plan( cache, files, fileMap );

			  AtomicBoolean stopSignal = new AtomicBoolean();
			  Callable<Void> planRunner = new PlanRunner( _plan, stopSignal, _profiler );
			  Future<Void>[] futures = new Future[_concurrencyLevel];
			  for ( int i = 0; i < _concurrencyLevel; i++ )
			  {
					futures[i] = _executorService.submit( planRunner );
			  }

			  if ( _preparation != null )
			  {
					_preparation.run( cache, this._fs, _plan.FilesTouched );
			  }

			  adversary.ProbabilityFactor = 1.0;

			  _plan.start();

			  long deadlineMillis = DateTimeHelper.CurrentUnixTimeMillis() + unit.toMillis(timeout);
			  long now;
			  try
			  {
					foreach ( Future<Void> future in futures )
					{
						 now = DateTimeHelper.CurrentUnixTimeMillis();
						 if ( deadlineMillis < now )
						 {
							  throw new TimeoutException();
						 }
						 future.get( deadlineMillis - now, TimeUnit.MILLISECONDS );
					}
					adversary.ProbabilityFactor = 0.0;
					RunVerificationPhase( cache );
			  }
			  finally
			  {
					stopSignal.set( true );
					adversary.ProbabilityFactor = 0.0;
					try
					{
						 foreach ( Future<Void> future in futures )
						 {
							  future.get( 10, TimeUnit.SECONDS );
						 }
					}
					catch ( Exception e ) when ( e is InterruptedException || e is TimeoutException )
					{
						 foreach ( Future<Void> future in futures )
						 {
							  future.cancel( true );
						 }
						 e.printStackTrace();
					}

					try
					{
						 _plan.close();
						 cache.Close();
						 jobScheduler.close();

						 if ( this._fs is EphemeralFileSystemAbstraction )
						 {
							  this._fs.Dispose();
							  this._fs = new EphemeralFileSystemAbstraction();
						 }
						 else
						 {
							  foreach ( File file in files )
							  {
									file.delete();
							  }
						 }
					}
					catch ( IOException e )
					{
						 Console.WriteLine( e.ToString() );
						 Console.Write( e.StackTrace );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void runVerificationPhase(Neo4Net.io.pagecache.impl.muninn.MuninnPageCache cache) throws Exception
		 private void RunVerificationPhase( MuninnPageCache cache )
		 {
			  if ( _verification != null )
			  {
					cache.FlushAndForce(); // Clears any stray evictor exceptions
					_verification.run( cache, this._fs, _plan.FilesTouched );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File[] buildFileNames() throws java.io.IOException
		 private File[] BuildFileNames()
		 {
			  string s = "abcdefghijklmnopqrstuvwxyz";
			  File[] files = new File[s.Length];
			  TestDirectory testDirectory = TestDirectory.testDirectory( typeof( RandomPageCacheTestHarness ), _fs );
			  File @base = testDirectory.PrepareDirectoryForTest( "random-pagecache-test-harness" );
			  for ( int i = 0; i < s.Length; i++ )
			  {
					files[i] = ( new File( @base, s.Substring( i, 1 ) ) ).CanonicalFile;
					_fs.mkdirs( files[i].ParentFile );
					StoreChannel channel = _fs.open( files[i], OpenMode.READ_WRITE );
					channel.Truncate( 0 );
					channel.close();
			  }
			  return files;
		 }

		 private Plan Plan( MuninnPageCache cache, File[] files, IDictionary<File, PagedFile> fileMap )
		 {
			  Action[] plan = new Action[_commandCount];

			  int[] commandWeights = ComputeCommandWeights();
			  int commandWeightSum = Sum( commandWeights );
			  Random rng = new Random( _randomSeed );
			  CommandPrimer primer = new CommandPrimer( rng, cache, files, fileMap, _filePageCount, _filePageSize, _recordFormat );

			  for ( int i = 0; i < plan.Length; i++ )
			  {
					Command command = PickCommand( rng.Next( commandWeightSum ), commandWeights );
					Action action = primer.Prime( command );
					plan[i] = action;
					if ( action == null )
					{
						 i--;
					}
			  }

			  return new Plan( plan, fileMap, primer.MappedFiles, primer.FilesTouched );
		 }

		 private int[] ComputeCommandWeights()
		 {
			  Command[] commands = Command.values();
			  int[] weights = new int[commands.Length];

			  int @base = 100_000_000;
			  for ( int i = 0; i < commands.Length; i++ )
			  {
					weights[i] = ( int )( @base * _commandProbabilityFactors[i] );
			  }

			  return weights;
		 }

		 private int Sum( int[] xs )
		 {
			  int sum = 0;
			  foreach ( int x in xs )
			  {
					sum += x;
			  }
			  return sum;
		 }

		 private Command PickCommand( int randomPick, int[] commandWeights )
		 {
			  for ( int i = 0; i < commandWeights.Length; i++ )
			  {
					randomPick -= commandWeights[i];
					if ( randomPick < 0 )
					{
						 return Command.values()[i];
					}
			  }
			  throw new AssertionError( "Tried to pick randomPick = " + randomPick + " from weights = " + Arrays.ToString( commandWeights ) );
		 }
	}

}