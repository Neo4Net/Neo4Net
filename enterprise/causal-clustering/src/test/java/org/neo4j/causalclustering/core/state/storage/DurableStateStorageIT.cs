using System;

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
namespace Org.Neo4j.causalclustering.core.state.storage
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using CountingAdversary = Org.Neo4j.Adversaries.CountingAdversary;
	using MethodGuardedAdversary = Org.Neo4j.Adversaries.MethodGuardedAdversary;
	using AdversarialFileSystemAbstraction = Org.Neo4j.Adversaries.fs.AdversarialFileSystemAbstraction;
	using EphemeralFileSystemAbstraction = Org.Neo4j.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using SelectiveFileSystemAbstraction = Org.Neo4j.Graphdb.mockfs.SelectiveFileSystemAbstraction;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using OpenMode = Org.Neo4j.Io.fs.OpenMode;
	using StoreChannel = Org.Neo4j.Io.fs.StoreChannel;
	using LifeSupport = Org.Neo4j.Kernel.Lifecycle.LifeSupport;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using ReadableChannel = Org.Neo4j.Storageengine.Api.ReadableChannel;
	using WritableChannel = Org.Neo4j.Storageengine.Api.WritableChannel;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Org.Neo4j.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThanOrEqualTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class DurableStateStorageIT
	{
		private bool InstanceFieldsInitialized = false;

		public DurableStateStorageIT()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _fileSystemRule ).around( _testDir );
		}


		 private readonly TestDirectory _testDir = TestDirectory.testDirectory();
		 private readonly EphemeralFileSystemRule _fileSystemRule = new EphemeralFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(fileSystemRule).around(testDir);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRecoverAfterCrashUnderLoad() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRecoverAfterCrashUnderLoad()
		 {
			  EphemeralFileSystemAbstraction @delegate = _fileSystemRule.get();
			  AdversarialFileSystemAbstraction fsa = new AdversarialFileSystemAbstraction( new MethodGuardedAdversary( new CountingAdversary( 100, true ), typeof( StoreChannel ).GetMethod( "writeAll", typeof( ByteBuffer ) ) ), @delegate );

			  long lastValue = 0;
			  try
			  {
					  using ( LongState persistedState = new LongState( fsa, _testDir.directory(), 14 ) )
					  {
						while ( true ) // it will break from the Exception that AFS will throw
						{
							 long tempValue = lastValue + 1;
							 persistedState.TheState = tempValue;
							 lastValue = tempValue;
						}
					  }
			  }
			  catch ( Exception expected )
			  {
					EnsureStackTraceContainsExpectedMethod( expected.StackTrace, "writeAll" );
			  }

			  using ( LongState restoredState = new LongState( @delegate, _testDir.directory(), 4 ) )
			  {
					assertEquals( lastValue, restoredState.TheState );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProperlyRecoveryAfterCrashOnFileCreationDuringRotation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProperlyRecoveryAfterCrashOnFileCreationDuringRotation()
		 {
			  EphemeralFileSystemAbstraction normalFSA = _fileSystemRule.get();
			  /*
			   * Magic number warning. For a rotation threshold of 14, 998 operations on file A falls on truncation of the
			   * file during rotation. This has been discovered via experimentation. The end result is that there is a
			   * failure to create the file to rotate to. This should be recoverable.
			   */
			  AdversarialFileSystemAbstraction breakingFSA = new AdversarialFileSystemAbstraction( new MethodGuardedAdversary( new CountingAdversary( 20, true ), typeof( FileSystemAbstraction ).GetMethod( "truncate", typeof( File ), typeof( long ) ) ), normalFSA );
			  SelectiveFileSystemAbstraction combinedFSA = new SelectiveFileSystemAbstraction( new File( new File( _testDir.directory(), "long-state" ), "long.a" ), breakingFSA, normalFSA );

			  long lastValue = 0;
			  try
			  {
					  using ( LongState persistedState = new LongState( combinedFSA, _testDir.directory(), 14 ) )
					  {
						while ( true ) // it will break from the Exception that AFS will throw
						{
							 long tempValue = lastValue + 1;
							 persistedState.TheState = tempValue;
							 lastValue = tempValue;
						}
					  }
			  }
			  catch ( Exception expected )
			  {
					// this stack trace should contain FSA.truncate()
					EnsureStackTraceContainsExpectedMethod( expected.StackTrace, "truncate" );
			  }

			  using ( LongState restoredState = new LongState( normalFSA, _testDir.directory(), 14 ) )
			  {
					assertEquals( lastValue, restoredState.TheState );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProperlyRecoveryAfterCrashOnFileForceDuringWrite() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProperlyRecoveryAfterCrashOnFileForceDuringWrite()
		 {
			  EphemeralFileSystemAbstraction normalFSA = _fileSystemRule.get();
			  /*
			   * Magic number warning. For a rotation threshold of 14, 990 operations on file A falls on a force() of the
			   * current active file. This has been discovered via experimentation. The end result is that there is a
			   * flush (but not write) a value. This should be recoverable. Interestingly, the failure semantics are a bit
			   * unclear on what should happen to that value. We assume that exception during persistence requires recovery
			   * to discover if the last argument made it to disk or not. Since we use an EFSA, force is not necessary and
			   * the value that caused the failure is actually "persisted" and recovered.
			   */
			  AdversarialFileSystemAbstraction breakingFSA = new AdversarialFileSystemAbstraction( new MethodGuardedAdversary( new CountingAdversary( 40, true ), typeof( StoreChannel ).GetMethod( "force", typeof( bool ) ) ), normalFSA );
			  SelectiveFileSystemAbstraction combinedFSA = new SelectiveFileSystemAbstraction( new File( new File( _testDir.directory(), "long-state" ), "long.a" ), breakingFSA, normalFSA );

			  long lastValue = 0;

			  try
			  {
					  using ( LongState persistedState = new LongState( combinedFSA, _testDir.directory(), 14 ) )
					  {
						while ( true ) // it will break from the Exception that AFS will throw
						{
							 long tempValue = lastValue + 1;
							 persistedState.TheState = tempValue;
							 lastValue = tempValue;
						}
					  }
			  }
			  catch ( Exception expected )
			  {
					// this stack trace should contain force()
					EnsureStackTraceContainsExpectedMethod( expected.StackTrace, "force" );
			  }

			  using ( LongState restoredState = new LongState( normalFSA, _testDir.directory(), 14 ) )
			  {
					assertThat( restoredState.TheState, greaterThanOrEqualTo( lastValue ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProperlyRecoveryAfterCrashingDuringRecovery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProperlyRecoveryAfterCrashingDuringRecovery()
		 {
			  EphemeralFileSystemAbstraction normalFSA = _fileSystemRule.get();

			  long lastValue = 0;

			  using ( LongState persistedState = new LongState( normalFSA, _testDir.directory(), 14 ) )
			  {
					for ( int i = 0; i < 100; i++ )
					{
						 long tempValue = lastValue + 1;
						 persistedState.TheState = tempValue;
						 lastValue = tempValue;
					}
			  }

			  try
			  {
					// We create a new state that will attempt recovery. The AFS will make it fail on open() of one of the files
					new LongState( new AdversarialFileSystemAbstraction( new MethodGuardedAdversary( new CountingAdversary( 1, true ), typeof( FileSystemAbstraction ).GetMethod( "open", typeof( File ), typeof( OpenMode ) ) ), normalFSA ), _testDir.directory(), 14 );
					fail( "Should have failed recovery" );
			  }
			  catch ( Exception expected )
			  {
					// this stack trace should contain open()
					EnsureStackTraceContainsExpectedMethod( expected.InnerException.StackTrace, "open" );
			  }

			  // Recovery over the normal filesystem after a failed recovery should proceed correctly
			  using ( LongState recoveredState = new LongState( normalFSA, _testDir.directory(), 14 ) )
			  {
					assertThat( recoveredState.TheState, greaterThanOrEqualTo( lastValue ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldProperlyRecoveryAfterCloseOnActiveFileDuringRotation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldProperlyRecoveryAfterCloseOnActiveFileDuringRotation()
		 {
			  EphemeralFileSystemAbstraction normalFSA = _fileSystemRule.get();
			  AdversarialFileSystemAbstraction breakingFSA = new AdversarialFileSystemAbstraction( new MethodGuardedAdversary( new CountingAdversary( 5, true ), typeof( StoreChannel ).GetMethod( "close" ) ), normalFSA );
			  SelectiveFileSystemAbstraction combinedFSA = new SelectiveFileSystemAbstraction( new File( new File( _testDir.directory(), "long-state" ), "long.a" ), breakingFSA, normalFSA );

			  long lastValue = 0;
			  try
			  {
					  using ( LongState persistedState = new LongState( combinedFSA, _testDir.directory(), 14 ) )
					  {
						while ( true ) // it will break from the Exception that AFS will throw
						{
							 long tempValue = lastValue + 1;
							 persistedState.TheState = tempValue;
							 lastValue = tempValue;
						}
					  }
			  }
			  catch ( Exception expected )
			  {
					// this stack trace should contain close()
					EnsureStackTraceContainsExpectedMethod( expected.StackTrace, "close" );
			  }

			  using ( LongState restoredState = new LongState( normalFSA, _testDir.directory(), 14 ) )
			  {
					assertThat( restoredState.TheState, greaterThanOrEqualTo( lastValue ) );
			  }
		 }

		 private void EnsureStackTraceContainsExpectedMethod( StackTraceElement[] stackTrace, string expectedMethodName )
		 {
			  foreach ( StackTraceElement stackTraceElement in stackTrace )
			  {
					if ( stackTraceElement.MethodName.Equals( expectedMethodName ) )
					{
						 return;
					}
			  }
			  fail( "Method " + expectedMethodName + " was not part of the failure stack trace." );
		 }

		 private class LongState : AutoCloseable
		 {
			  internal const string FILENAME = "long";
			  internal readonly DurableStateStorage<long> StateStorage;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal long TheStateConflict = -1;
			  internal LifeSupport LifeSupport = new LifeSupport();

			  internal LongState( FileSystemAbstraction fileSystemAbstraction, File stateDir, int numberOfEntriesBeforeRotation )
			  {
					LifeSupport.start();

					StateMarshal<long> byteBufferMarshal = new SafeStateMarshalAnonymousInnerClass( this );

					this.StateStorage = LifeSupport.add( new DurableStateStorage<>( fileSystemAbstraction, stateDir, FILENAME, byteBufferMarshal, numberOfEntriesBeforeRotation, NullLogProvider.Instance ) );

					this.TheStateConflict = this.StateStorage.InitialState;
			  }

			  private class SafeStateMarshalAnonymousInnerClass : SafeStateMarshal<long>
			  {
				  private readonly LongState _outerInstance;

				  public SafeStateMarshalAnonymousInnerClass( LongState outerInstance )
				  {
					  this.outerInstance = outerInstance;
				  }

				  public override long? startState()
				  {
						return 0L;
				  }

				  public override long ordinal( long? aLong )
				  {
						return aLong.Value;
				  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void marshal(System.Nullable<long> aLong, org.neo4j.storageengine.api.WritableChannel channel) throws java.io.IOException
				  public override void marshal( long? aLong, WritableChannel channel )
				  {
						channel.PutLong( aLong.Value );
				  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public System.Nullable<long> unmarshal0(org.neo4j.storageengine.api.ReadableChannel channel) throws java.io.IOException
				  public override long? unmarshal0( ReadableChannel channel )
				  {
						return channel.Long;
				  }
			  }

			  internal virtual long TheState
			  {
				  get
				  {
						return TheStateConflict;
				  }
				  set
				  {
						StateStorage.persistStoreData( value );
						this.TheStateConflict = value;
				  }
			  }


			  public override void Close()
			  {
					LifeSupport.shutdown();
			  }
		 }
	}

}