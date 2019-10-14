using System;
using System.Threading;

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
namespace Neo4Net.Kernel.impl.transaction
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using PropertyContainer = Neo4Net.Graphdb.PropertyContainer;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using LogEntry = Neo4Net.Kernel.impl.transaction.log.entry.LogEntry;
	using LogFiles = Neo4Net.Kernel.impl.transaction.log.files.LogFiles;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using Neo4Net.Test.LogTestUtils;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.LogTestUtils.filterNeostoreLogicalLog;

	/// <summary>
	/// Asserts that pure read operations does not write records to logical or transaction logs.
	/// </summary>
	public class ReadTransactionLogWritingTest
	{
		private bool InstanceFieldsInitialized = false;

		public ReadTransactionLogWritingTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_label = _label( "Test" );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.DatabaseRule dbr = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public readonly DatabaseRule Dbr = new ImpermanentDatabaseRule();

		 private Label _label;
		 private Node _node;
		 private Relationship _relationship;
		 private long _logEntriesWrittenBeforeReadOperations;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void createDataset()
		 public virtual void CreateDataset()
		 {
			  GraphDatabaseAPI db = Dbr.GraphDatabaseAPI;
			  using ( Transaction tx = Db.beginTx() )
			  {
					_node = Db.createNode( _label );
					_node.setProperty( "short", 123 );
					_node.setProperty( "long", LongString( 300 ) );
					_relationship = _node.createRelationshipTo( Db.createNode(), MyRelTypes.TEST );
					_relationship.setProperty( "short", 123 );
					_relationship.setProperty( "long", LongString( 300 ) );
					tx.Success();
			  }
			  _logEntriesWrittenBeforeReadOperations = CountLogEntries();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotWriteAnyLogCommandInPureReadTransaction()
		 public virtual void ShouldNotWriteAnyLogCommandInPureReadTransaction()
		 {
			  // WHEN
			  ExecuteTransaction( Relationships );
			  ExecuteTransaction( Properties );
			  ExecuteTransaction( ById );
			  ExecuteTransaction( NodesFromRelationship );

			  // THEN
			  long actualCount = CountLogEntries();
			  assertEquals( "There were " + ( actualCount - _logEntriesWrittenBeforeReadOperations ) + " log entries written during one or more pure read transactions", _logEntriesWrittenBeforeReadOperations, actualCount );
		 }

		 private long CountLogEntries()
		 {
			  GraphDatabaseAPI db = Dbr.GraphDatabaseAPI;
			  FileSystemAbstraction fs = Db.DependencyResolver.resolveDependency( typeof( FileSystemAbstraction ) );
			  LogFiles logFiles = Db.DependencyResolver.resolveDependency( typeof( LogFiles ) );
			  try
			  {
					CountingLogHook<LogEntry> logicalLogCounter = new CountingLogHook<LogEntry>();
					filterNeostoreLogicalLog( logFiles, fs, logicalLogCounter );

					long txLogRecordCount = logFiles.LogFileInformation.LastEntryId;

					return logicalLogCounter.Count + txLogRecordCount;
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

		 private string LongString( int length )
		 {
			  char[] characters = new char[length];
			  for ( int i = 0; i < length; i++ )
			  {
					characters[i] = ( char )( 'a' + i % 10 );
			  }
			  return new string( characters );
		 }

		 private void ExecuteTransaction( ThreadStart runnable )
		 {
			  ExecuteTransaction( runnable, true );
			  ExecuteTransaction( runnable, false );
		 }

		 private void ExecuteTransaction( ThreadStart runnable, bool success )
		 {
			  using ( Transaction tx = Dbr.GraphDatabaseAPI.beginTx() )
			  {
					runnable.run();
					if ( success )
					{
						 tx.Success();
					}
			  }
		 }

		 private ThreadStart Relationships
		 {
			 get
			 {
				  return () => assertEquals(1, Iterables.count(_node.Relationships));
			 }
		 }

		 private ThreadStart NodesFromRelationship
		 {
			 get
			 {
				  return () =>
				  {
					_relationship.EndNode;
					_relationship.StartNode;
					_relationship.Nodes;
					_relationship.getOtherNode( _node );
				  };
			 }
		 }

		 private ThreadStart ById
		 {
			 get
			 {
				  return () =>
				  {
					Dbr.GraphDatabaseAPI.getNodeById( _node.Id );
					Dbr.GraphDatabaseAPI.getRelationshipById( _relationship.Id );
				  };
			 }
		 }

		 private ThreadStart Properties
		 {
			 get
			 {
				  return new RunnableAnonymousInnerClass( this );
			 }
		 }

		 private class RunnableAnonymousInnerClass : ThreadStart
		 {
			 private readonly ReadTransactionLogWritingTest _outerInstance;

			 public RunnableAnonymousInnerClass( ReadTransactionLogWritingTest outerInstance )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public void run()
			 {
				  getAllProperties( _outerInstance.node );
				  getAllProperties( _outerInstance.relationship );
			 }

			 private void getAllProperties( PropertyContainer entity )
			 {
				  foreach ( string key in entity.PropertyKeys )
				  {
						entity.GetProperty( key );
				  }
			 }
		 }
	}

}