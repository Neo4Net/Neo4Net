using System.Collections.Generic;

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
namespace Org.Neo4j.tools.rebuild
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using RuleChain = org.junit.rules.RuleChain;
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using InconsistentStoreException = Org.Neo4j.Consistency.checking.InconsistentStoreException;
	using ConsistencySummaryStatistics = Org.Neo4j.Consistency.report.ConsistencySummaryStatistics;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using Org.Neo4j.Graphdb;
	using Org.Neo4j.Graphdb;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using FileUtils = Org.Neo4j.Io.fs.FileUtils;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using MetaDataStore = Org.Neo4j.Kernel.impl.store.MetaDataStore;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using DbRepresentation = Org.Neo4j.Test.DbRepresentation;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using SuppressOutput = Org.Neo4j.Test.rule.SuppressOutput;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.TransactionIdStore_Fields.BASE_TX_ID;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class RebuildFromLogsTest
	public class RebuildFromLogsTest
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _dir ).around( _suppressOutput ).around( _fileSystemRule ).around( _expectedException );
		}

		 private readonly TestDirectory _dir = TestDirectory.testDirectory();
		 private readonly SuppressOutput _suppressOutput = SuppressOutput.suppressAll();
		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();
		 private readonly ExpectedException _expectedException = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(dir).around(suppressOutput).around(fileSystemRule).around(expectedException);
		 public RuleChain RuleChain;

		 private readonly Transaction[] _work;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{0}") public static java.util.Collection<WorkLog> commands()
		 public static ICollection<WorkLog> Commands()
		 {
			  return WorkLog.Combinations();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRebuildFromLog() throws Exception, org.neo4j.consistency.checking.InconsistentStoreException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRebuildFromLog()
		 {
			  // given
			  File prototypePath = PrototypePath;
			  PopulatePrototype( prototypePath );

			  // when
			  File rebuildPath = RebuilPath;
			  ( new RebuildFromLogs( _fileSystemRule.get() ) ).rebuild(prototypePath, rebuildPath, BASE_TX_ID);

			  // then
			  assertEquals( GetDbRepresentation( prototypePath ), GetDbRepresentation( rebuildPath ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void failRebuildFromLogIfStoreIsInconsistentAfterRebuild() throws org.neo4j.consistency.checking.InconsistentStoreException, Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FailRebuildFromLogIfStoreIsInconsistentAfterRebuild()
		 {
			  File prototypePath = PrototypePath;
			  PopulatePrototype( prototypePath );

			  // when
			  File rebuildPath = RebuilPath;
			  _expectedException.expect( typeof( InconsistentStoreException ) );
			  RebuildFromLogs rebuildFromLogs = new TestRebuildFromLogs( this, _fileSystemRule.get() );
			  rebuildFromLogs.Rebuild( prototypePath, rebuildPath, BASE_TX_ID );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRebuildFromLogUpToATx() throws Exception, org.neo4j.consistency.checking.InconsistentStoreException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRebuildFromLogUpToATx()
		 {
			  // given
			  File prototypePath = PrototypePath;
			  long txId = PopulatePrototype( prototypePath );

			  File copy = new File( _dir.databaseDir(), "copy" );
			  FileUtils.copyRecursively( prototypePath, copy );
			  GraphDatabaseAPI db = db( copy );
			  try
			  {
					  using ( Org.Neo4j.Graphdb.Transaction tx = Db.beginTx() )
					  {
						Db.createNode();
						tx.Success();
					  }
			  }
			  finally
			  {
					Db.shutdown();
			  }

			  // when
			  File rebuildPath = RebuilPath;
			  ( new RebuildFromLogs( _fileSystemRule.get() ) ).rebuild(copy, rebuildPath, txId);

			  // then
			  assertEquals( GetDbRepresentation( prototypePath ), GetDbRepresentation( rebuildPath ) );
		 }

		 private File RebuilPath
		 {
			 get
			 {
				  return new File( _dir.directory(), "rebuild" );
			 }
		 }

		 private File PrototypePath
		 {
			 get
			 {
				  return new File( _dir.directory(), "prototype" );
			 }
		 }

		 private long PopulatePrototype( File prototypePath )
		 {
			  GraphDatabaseAPI prototype = Db( prototypePath );
			  long txId;
			  try
			  {
					foreach ( Transaction transaction in _work )
					{
						 transaction.applyTo( prototype );
					}
			  }
			  finally
			  {
					txId = prototype.DependencyResolver.resolveDependency( typeof( MetaDataStore ) ).LastCommittedTransactionId;
					prototype.Shutdown();
			  }
			  return txId;
		 }

		 private static DbRepresentation GetDbRepresentation( File path )
		 {
			  return DbRepresentation.of( path );
		 }

		 private static GraphDatabaseAPI Db( File storeDir )
		 {
			  return ( GraphDatabaseAPI ) ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(storeDir);
		 }

		 internal sealed class Transaction
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           CREATE_NODE { void applyTx(org.neo4j.graphdb.GraphDatabaseService graphDb) { graphDb.createNode(); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           CREATE_NODE_WITH_PROPERTY { void applyTx(org.neo4j.graphdb.GraphDatabaseService graphDb) { graphDb.createNode().setProperty(name(), "value"); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           SET_PROPERTY(CREATE_NODE) { void applyTx(org.neo4j.graphdb.GraphDatabaseService graphDb) { firstNode(graphDb).setProperty(name(), "value"); } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           CHANGE_PROPERTY(CREATE_NODE_WITH_PROPERTY) { void applyTx(org.neo4j.graphdb.GraphDatabaseService graphDb) { org.neo4j.graphdb.ResourceIterable<org.neo4j.graphdb.Node> nodes = graphDb.getAllNodes(); try(org.neo4j.graphdb.ResourceIterator<org.neo4j.graphdb.Node> iterator = nodes.iterator()) { while (iterator.hasNext()) { org.neo4j.graphdb.Node node = iterator.next(); if("value".equals(node.getProperty(CREATE_NODE_WITH_PROPERTY.name(), null))) { node.setProperty(CREATE_NODE_WITH_PROPERTY.name(), "other"); break; } } } } },
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           EXPLICIT_INDEX_NODE(CREATE_NODE) { void applyTx(org.neo4j.graphdb.GraphDatabaseService graphDb) { org.neo4j.graphdb.Node node = firstNode(graphDb); graphDb.index().forNodes(name()).add(node, "foo", "bar"); } };

			  private static readonly IList<Transaction> valueList = new List<Transaction>();

			  static Transaction()
			  {
				  valueList.Add( CREATE_NODE );
				  valueList.Add( CREATE_NODE_WITH_PROPERTY );
				  valueList.Add( SET_PROPERTY );
				  valueList.Add( CHANGE_PROPERTY );
				  valueList.Add( EXPLICIT_INDEX_NODE );
			  }

			  public enum InnerEnum
			  {
				  CREATE_NODE,
				  CREATE_NODE_WITH_PROPERTY,
				  SET_PROPERTY,
				  CHANGE_PROPERTY,
				  EXPLICIT_INDEX_NODE
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private Transaction( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal static Org.Neo4j.Graphdb.Node FirstNode( Org.Neo4j.Graphdb.GraphDatabaseService graphDb )
			  {
					return Iterables.firstOrNull( graphDb.AllNodes );
			  }

			  internal readonly Transaction[] dependencies;

			  internal Transaction( string name, InnerEnum innerEnum, params Transaction[] dependencies )
			  {
					this._dependencies = dependencies;

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal void ApplyTo( Org.Neo4j.Graphdb.GraphDatabaseService graphDb )
			  {
					using ( Org.Neo4j.Graphdb.Transaction tx = graphDb.BeginTx() )
					{
						 ApplyTx( graphDb );

						 tx.Success();
					}
			  }

			  internal void ApplyTx( Org.Neo4j.Graphdb.GraphDatabaseService graphDb )
			  {
			  }

			 public static IList<Transaction> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public override string ToString()
			 {
				 return nameValue;
			 }

			 public static Transaction valueOf( string name )
			 {
				 foreach ( Transaction enumInstance in Transaction.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 internal class WorkLog
		 {
			  internal static readonly WorkLog Base = new WorkLog( EnumSet.noneOf( typeof( Transaction ) ) );
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly EnumSet<Transaction> TransactionsConflict;

			  internal WorkLog( EnumSet<Transaction> transactions )
			  {
					this.TransactionsConflict = transactions;
			  }

			  public override bool Equals( object that )
			  {
					return this == that || that is WorkLog && TransactionsConflict.Equals( ( ( WorkLog ) that ).TransactionsConflict );
			  }

			  public override int GetHashCode()
			  {
					return TransactionsConflict.GetHashCode();
			  }

			  public override string ToString()
			  {
					return TransactionsConflict.ToString();
			  }

			  internal virtual Transaction[] Transactions()
			  {
					return TransactionsConflict.toArray( new Transaction[TransactionsConflict.size()] );
			  }

			  internal static ISet<WorkLog> Combinations()
			  {
					ISet<WorkLog> combinations = Collections.newSetFromMap( new LinkedHashMap<WorkLog>() );
					foreach ( Transaction transaction in Transaction.values() )
					{
						 combinations.Add( Base.extend( transaction ) );
					}
					foreach ( Transaction transaction in Transaction.values() )
					{
						 foreach ( WorkLog combination in new List<>( combinations ) )
						 {
							  combinations.Add( combination.Extend( transaction ) );
						 }
					}
					return combinations;
			  }

			  internal virtual WorkLog Extend( Transaction transaction )
			  {
					EnumSet<Transaction> @base = EnumSet.copyOf( TransactionsConflict );
					Collections.addAll( @base, transaction.dependencies );
					@base.add( transaction );
					return new WorkLog( @base );
			  }
		 }

		 public RebuildFromLogsTest( WorkLog work )
		 {
			 if ( !InstanceFieldsInitialized )
			 {
				 InitializeInstanceFields();
				 InstanceFieldsInitialized = true;
			 }
			  this._work = work.Transactions();
		 }

		 private class TestRebuildFromLogs : RebuildFromLogs
		 {
			 private readonly RebuildFromLogsTest _outerInstance;

			  internal TestRebuildFromLogs( RebuildFromLogsTest outerInstance, FileSystemAbstraction fs ) : base( fs )
			  {
				  this._outerInstance = outerInstance;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void checkConsistency(java.io.File target, org.neo4j.io.pagecache.PageCache pageCache) throws org.neo4j.consistency.checking.InconsistentStoreException
			  internal override void CheckConsistency( File target, PageCache pageCache )
			  {
					throw new InconsistentStoreException( new ConsistencySummaryStatistics() );
			  }
		 }
	}

}