using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.Procedure
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Ignore = org.junit.Ignore;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using TemporaryFolder = org.junit.rules.TemporaryFolder;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Path = Neo4Net.Graphdb.Path;
	using QueryExecutionException = Neo4Net.Graphdb.QueryExecutionException;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using Result = Neo4Net.Graphdb.Result;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using JarBuilder = Neo4Net.Kernel.impl.proc.JarBuilder;
	using Log = Neo4Net.Logging.Log;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsEqual.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;

	public class UserAggregationFunctionIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.TemporaryFolder plugins = new org.junit.rules.TemporaryFolder();
		 public TemporaryFolder Plugins = new TemporaryFolder();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();

		 private GraphDatabaseService _db;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleSingleStringArgumentAggregationFunction()
		 public virtual void ShouldHandleSingleStringArgumentAggregationFunction()
		 {
			  // Given
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.execute( "CREATE ({ prop:'foo'})" );
					_db.execute( "CREATE ({ prop:'foo'})" );
					_db.execute( "CREATE ({ prop:'bar'})" );
					_db.execute( "CREATE ({prop:'baz'})" );
					_db.execute( "CREATE ()" );
					tx.Success();
			  }

			  // When
			  Result result = _db.execute( "MATCH (n) RETURN org.neo4j.procedure.count(n.prop) AS count" );

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( result.Next(), equalTo(map("count", 4L)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( result.HasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleSingleStringArgumentAggregationFunctionAndGroupingKey()
		 public virtual void ShouldHandleSingleStringArgumentAggregationFunctionAndGroupingKey()
		 {
			  // Given
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.execute( "CREATE ({prop1:42, prop2:'foo'})" );
					_db.execute( "CREATE ({prop1:42, prop2:'foo'})" );
					_db.execute( "CREATE ({prop1:42, prop2:'bar'})" );
					_db.execute( "CREATE ({prop1:1337, prop2:'baz'})" );
					_db.execute( "CREATE ({prop1:1337})" );
					tx.Success();
			  }

			  // When
			  Result result = _db.execute( "MATCH (n) RETURN n.prop1, org.neo4j.procedure.count(n.prop2) AS count" );

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( result.Next(), equalTo(map("n.prop1", 42L, "count", 3L)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( result.Next(), equalTo(map("n.prop1", 1337L, "count", 1L)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( result.HasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailNicelyWhenInvalidRuntimeType()
		 public virtual void ShouldFailNicelyWhenInvalidRuntimeType()
		 {
			  // Given
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.execute( "CREATE ({ prop:'foo'})" );
					_db.execute( "CREATE ({ prop:'foo'})" );
					_db.execute( "CREATE ({ prop:'bar'})" );
					_db.execute( "CREATE ({prop:42})" );
					_db.execute( "CREATE ()" );
					tx.Success();
			  }

			  // Expect
			  Exception.expect( typeof( QueryExecutionException ) );
			  Exception.expectMessage( "Can't coerce `Long(42)` to String" );

			  // When
			  _db.execute( "MATCH (n) RETURN org.neo4j.procedure.count(n.prop) AS count" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleNodeArgumentAggregationFunction()
		 public virtual void ShouldHandleNodeArgumentAggregationFunction()
		 {
			  // Given
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.execute( "CREATE ({ level:42})" );
					_db.execute( "CREATE ({ level:1337})" );
					_db.execute( "CREATE ({ level:0})" );
					_db.execute( "CREATE ()" );
					tx.Success();
			  }

			  // When
			  Result result = _db.execute( "MATCH (n) WITH org.neo4j.procedure.findBestNode(n) AS best RETURN best.level AS level" );

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( result.Next(), equalTo(map("level", 1337L)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( result.HasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleRelationshipArgumentAggregationFunction()
		 public virtual void ShouldHandleRelationshipArgumentAggregationFunction()
		 {
			  // Given
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.execute( "CREATE ()-[:T {level:42}]->()" );
					_db.execute( "CREATE ()-[:T {level:1337}]->()" );
					_db.execute( "CREATE ()-[:T {level:2}]->()" );
					_db.execute( "CREATE ()-[:T]->()" );
					tx.Success();
			  }

			  // When
			  Result result = _db.execute( "MATCH ()-[r]->() WITH org.neo4j.procedure.findBestRel(r) AS best RETURN best.level AS level" );

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( result.Next(), equalTo(map("level", 1337L)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( result.HasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandlePathArgumentAggregationFunction()
		 public virtual void ShouldHandlePathArgumentAggregationFunction()
		 {
			  // Given
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.execute( "CREATE ()-[:T]->()" );
					_db.execute( "CREATE ()-[:T]->()-[:T]->()" );
					_db.execute( "CREATE ()-[:T]->()-[:T]->()-[:T]->()" );
					tx.Success();
			  }

			  // When
			  Result result = _db.execute( "MATCH p=()-[:T*]->() WITH org.neo4j.procedure.longestPath(p) AS longest RETURN length(longest) AS " + "len" );

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( result.Next(), equalTo(map("len", 3L)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( result.HasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleNullPath()
		 public virtual void ShouldHandleNullPath()
		 {
			  // When
			  Result result = _db.execute( "MATCH p=()-[:T*]->() WITH org.neo4j.procedure.longestPath(p) AS longest RETURN longest" );

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( result.Next(), equalTo(map("longest", null)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( result.HasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleNumberArgumentAggregationFunction()
		 public virtual void ShouldHandleNumberArgumentAggregationFunction()
		 {
			  // Given, When
			  Result result = _db.execute( "UNWIND [43, 42.5, 41.9, 1337] AS num RETURN org.neo4j.procedure.near42(num) AS closest" );

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( result.Next(), equalTo(map("closest", 41.9)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( result.HasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleDoubleArgumentAggregationFunction()
		 public virtual void ShouldHandleDoubleArgumentAggregationFunction()
		 {
			  // Given, When
			  Result result = _db.execute( "UNWIND [43, 42.5, 41.9, 1337] AS num RETURN org.neo4j.procedure.doubleAggregator(num) AS closest" );

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( result.Next(), equalTo(map("closest", 41.9)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( result.HasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleLongArgumentAggregationFunction()
		 public virtual void ShouldHandleLongArgumentAggregationFunction()
		 {
			  // Given, When
			  Result result = _db.execute( "UNWIND [43, 42.5, 41.9, 1337] AS num RETURN org.neo4j.procedure.longAggregator(num) AS closest" );

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( result.Next(), equalTo(map("closest", 42L)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( result.HasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleNoArgumentBooleanAggregationFunction()
		 public virtual void ShouldHandleNoArgumentBooleanAggregationFunction()
		 {
			  assertThat( _db.execute( "UNWIND [1,2] AS num RETURN org.neo4j.procedure.boolAggregator() AS wasCalled" ).next(), equalTo(map("wasCalled", true)) );
			  assertThat( _db.execute( "UNWIND [] AS num RETURN org.neo4j.procedure.boolAggregator() AS wasCalled" ).next(), equalTo(map("wasCalled", false)) );

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToUseAdbInFunction()
		 public virtual void ShouldBeAbleToUseAdbInFunction()
		 {
			  IList<Node> nodes = new List<Node>();
			  // Given
			  using ( Transaction tx = _db.beginTx() )
			  {
					nodes.Add( _db.createNode() );
					nodes.Add( _db.createNode() );
					nodes.Add( _db.createNode() );
					nodes.Add( _db.createNode() );
					tx.Success();
			  }

			  // When
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  Result result = _db.execute( "UNWIND $ids AS ids WITH org.neo4j.procedure.collectNode(ids) AS nodes RETURN nodes", map( "ids", nodes.Select( Node::getId ).ToList() ) );

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( result.Next(), equalTo(map("nodes", nodes)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( result.HasNext() );
		 }

		 //TODO unignore when we have updated front end dependency
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore public void shouldBeAbleToAccessPropertiesFromAggregatedValues()
		 public virtual void ShouldBeAbleToAccessPropertiesFromAggregatedValues()
		 {
			  // Given
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.execute( "CREATE (:User {country: 'Sweden'})" );
					_db.execute( "CREATE (:User {country: 'Sweden'})" );
					_db.execute( "CREATE (:User {country: 'Sweden'})" );
					_db.execute( "CREATE (:User {country: 'Sweden'})" );
					_db.execute( "CREATE (:User {country: 'Germany'})" );
					_db.execute( "CREATE (:User {country: 'Germany'})" );
					_db.execute( "CREATE (:User {country: 'Germany'})" );
					_db.execute( "CREATE (:User {country: 'Mexico'})" );
					_db.execute( "CREATE (:User {country: 'Mexico'})" );
					_db.execute( "CREATE (:User {country: 'South Korea'})" );
					tx.Success();
			  }

			  // When
			  IList<IDictionary<string, object>> result = Iterators.asList( _db.execute( "MATCH (u:User) RETURN u.country,count(*),org.neo4j.procedure.first(u).country AS first" ) );

			  // Then
			  assertThat( result, hasSize( 4 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  ( new JarBuilder() ).createJarFor(Plugins.newFile("myFunctions.jar"), typeof(ClassWithFunctions));
			  _db = ( new TestGraphDatabaseFactory() ).newImpermanentDatabaseBuilder().setConfig(GraphDatabaseSettings.plugin_dir, Plugins.Root.AbsolutePath).setConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).newGraphDatabase();

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  if ( this._db != null )
			  {
					this._db.shutdown();
			  }
		 }

		 public class ClassWithFunctions
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.graphdb.GraphDatabaseService db;
			  public GraphDatabaseService Db;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.logging.Log log;
			  public Log Log;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationFunction public CountAggregator count()
			  public virtual CountAggregator Count()
			  {
					return new CountAggregator();
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationFunction public RelAggregator findBestRel()
			  public virtual RelAggregator FindBestRel()
			  {
					return new RelAggregator();
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationFunction public LongestPathAggregator longestPath()
			  public virtual LongestPathAggregator LongestPath()
			  {
					return new LongestPathAggregator();
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationFunction public NodeAggregator findBestNode()
			  public virtual NodeAggregator FindBestNode()
			  {
					return new NodeAggregator();
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationFunction public DoubleAggregator doubleAggregator()
			  public virtual DoubleAggregator DoubleAggregator()
			  {
					return new DoubleAggregator();
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationFunction public LongAggregator longAggregator()
			  public virtual LongAggregator LongAggregator()
			  {
					return new LongAggregator();
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationFunction public BoolAggregator boolAggregator()
			  public virtual BoolAggregator BoolAggregator()
			  {
					return new BoolAggregator();
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationFunction public ClosestTo42Aggregator near42()
			  public virtual ClosestTo42Aggregator Near42()
			  {
					return new ClosestTo42Aggregator();
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationFunction public NodeFromIdAggregator collectNode()
			  public virtual NodeFromIdAggregator CollectNode()
			  {
					return new NodeFromIdAggregator( Db );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationFunction public FirstAggregator first()
			  public virtual FirstAggregator First()
			  {
					return new FirstAggregator();
			  }

			  public class FirstAggregator
			  {
					internal object First;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationUpdate public void update(@Name("item") Object o)
					public virtual void Update( object o )
					{
						 if ( First == null )
						 {
							  First = o;
						 }
					}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationResult public Object result()
					public virtual object Result()
					{
						 return First;
					}
			  }

			  public class NodeAggregator
			  {
					internal Node AggregateNode;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationUpdate public void update(@Name("node") org.neo4j.graphdb.Node node)
					public virtual void Update( Node node )
					{
						 if ( node != null )
						 {
							  long level = ( long ) node.GetProperty( "level", 0L );

							  if ( AggregateNode == null )
							  {
									AggregateNode = node;
							  }
							  else if ( level > ( long ) AggregateNode.getProperty( "level", 0L ) )
							  {
									AggregateNode = node;
							  }
						 }
					}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationResult public org.neo4j.graphdb.Node result()
					public virtual Node Result()
					{
						 return AggregateNode;
					}
			  }

			  public class RelAggregator
			  {
					internal Relationship AggregateRel;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationUpdate public void update(@Name("rel") org.neo4j.graphdb.Relationship rel)
					public virtual void Update( Relationship rel )
					{
						 if ( rel != null )
						 {
							  long level = ( long ) rel.GetProperty( "level", 0L );

							  if ( AggregateRel == null )
							  {
									AggregateRel = rel;
							  }
							  else if ( level > ( long ) AggregateRel.getProperty( "level", 0L ) )
							  {
									AggregateRel = rel;
							  }
						 }
					}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationResult public org.neo4j.graphdb.Relationship result()
					public virtual Relationship Result()
					{
						 return AggregateRel;
					}
			  }

			  public class LongestPathAggregator
			  {
					internal Path AggregatePath;
					internal int Longest;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationUpdate public void update(@Name("path") org.neo4j.graphdb.Path path)
					public virtual void Update( Path path )
					{
						 if ( path != null )
						 {
							  if ( path.Length() > Longest )
							  {
									Longest = path.Length();
									AggregatePath = path;
							  }
						 }
					}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationResult public org.neo4j.graphdb.Path result()
					public virtual Path Result()
					{
						 return AggregatePath;
					}
			  }

			  public class ClosestTo42Aggregator
			  {
					internal Number Closest;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationUpdate public void update(@Name("number") Number number)
					public virtual void Update( Number number )
					{
						 if ( number != null )
						 {
							  if ( Closest == null )
							  {
									Closest = number;
							  }
							  else if ( Math.Abs( number.doubleValue() - 42L ) < Math.Abs(Closest.doubleValue() - 42L) )
							  {
									Closest = number;
							  }
						 }
					}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationResult public Number result()
					public virtual Number Result()
					{
						 return Closest;
					}
			  }

			  public class DoubleAggregator
			  {
					internal double? Closest;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationUpdate public void update(@Name("double") System.Nullable<double> number)
					public virtual void Update( double? number )
					{
						 if ( number != null )
						 {
							  if ( Closest == null )
							  {
									Closest = number;
							  }
							  else if ( Math.Abs( number - 42L ) < Math.Abs( Closest - 42L ) )
							  {
									Closest = number;
							  }
						 }
					}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationResult public System.Nullable<double> result()
					public virtual double? Result()
					{
						 return Closest;
					}
			  }

			  public class LongAggregator
			  {
					internal long? Closest;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationUpdate public void update(@Name("long") System.Nullable<long> number)
					public virtual void Update( long? number )
					{
						 if ( number != null )
						 {
							  if ( Closest == null )
							  {
									Closest = number;
							  }
							  else if ( Math.Abs( number - 42L ) < Math.Abs( Closest - 42L ) )
							  {
									Closest = number;
							  }
						 }
					}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationResult public System.Nullable<long> result()
					public virtual long? Result()
					{
						 return Closest;
					}
			  }

			  public class CountAggregator
			  {
					internal long Count;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationUpdate public void update(@Name("in") String in)
					public virtual void Update( string @in )
					{
						 if ( !string.ReferenceEquals( @in, null ) )
						 {
							  Count += 1L;
						 }
					}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationResult public long result()
					public virtual long Result()
					{
						 return Count;
					}
			  }

			  public class BoolAggregator
			  {
					internal bool WasCalled;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationUpdate public void update()
					public virtual void Update()
					{
						 WasCalled = true;
					}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationResult public boolean result()
					public virtual bool Result()
					{
						 return WasCalled;
					}
			  }

			  public class NodeFromIdAggregator
			  {
					internal readonly IList<long> Ids = new List<long>();
					internal readonly GraphDatabaseService Gds;

					public NodeFromIdAggregator( GraphDatabaseService gds )
					{
						 this.Gds = gds;
					}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationUpdate public void update(@Name("id") long id)
					public virtual void Update( long id )
					{
						 Ids.Add( id );
					}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationResult public java.util.List<org.neo4j.graphdb.Node> result()
					public virtual IList<Node> Result()
					{
						 return Ids.Select( Gds.getNodeById ).ToList();
					}

			  }
		 }
	}

}