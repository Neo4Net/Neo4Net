using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Bolt.v1.runtime
{
	using Test = org.junit.Test;


	using BoltResult = Neo4Net.Bolt.runtime.BoltResult;
	using QueryResult = Neo4Net.Cypher.result.QueryResult;
	using ExecutionPlanDescription = Neo4Net.Graphdb.ExecutionPlanDescription;
	using InputPosition = Neo4Net.Graphdb.InputPosition;
	using QueryStatistics = Neo4Net.Graphdb.QueryStatistics;
	using NotificationCode = Neo4Net.Graphdb.impl.notification.NotificationCode;
	using TransactionalContext = Neo4Net.Kernel.impl.query.TransactionalContext;
	using AnyValue = Neo4Net.Values.AnyValue;
	using DoubleValue = Neo4Net.Values.Storable.DoubleValue;
	using MapValue = Neo4Net.Values.@virtual.MapValue;
	using MapValueBuilder = Neo4Net.Values.@virtual.MapValueBuilder;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.closeTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.QueryExecutionType.QueryType.READ_ONLY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.QueryExecutionType.QueryType.READ_WRITE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.QueryExecutionType.explained;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.QueryExecutionType.query;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.doubleValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.intValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.longValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.@virtual.VirtualValues.list;

	public class CypherAdapterStreamTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIncludeBasicMetadata() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIncludeBasicMetadata()
		 {
			  // Given
			  QueryStatistics queryStatistics = mock( typeof( QueryStatistics ) );
			  when( queryStatistics.ContainsUpdates() ).thenReturn(true);
			  when( queryStatistics.NodesCreated ).thenReturn( 1 );
			  when( queryStatistics.NodesDeleted ).thenReturn( 2 );
			  when( queryStatistics.RelationshipsCreated ).thenReturn( 3 );
			  when( queryStatistics.RelationshipsDeleted ).thenReturn( 4 );
			  when( queryStatistics.PropertiesSet ).thenReturn( 5 );
			  when( queryStatistics.IndexesAdded ).thenReturn( 6 );
			  when( queryStatistics.IndexesRemoved ).thenReturn( 7 );
			  when( queryStatistics.ConstraintsAdded ).thenReturn( 8 );
			  when( queryStatistics.ConstraintsRemoved ).thenReturn( 9 );
			  when( queryStatistics.LabelsAdded ).thenReturn( 10 );
			  when( queryStatistics.LabelsRemoved ).thenReturn( 11 );

			  QueryResult result = mock( typeof( QueryResult ) );
			  when( result.FieldNames() ).thenReturn(new string[0]);
			  when( result.ExecutionType() ).thenReturn(query(READ_WRITE));
			  when( result.QueryStatistics() ).thenReturn(queryStatistics);
			  when( result.Notifications ).thenReturn( Collections.emptyList() );

			  Clock clock = mock( typeof( Clock ) );
			  when( clock.millis() ).thenReturn(0L, 1337L);

			  TransactionalContext tc = mock( typeof( TransactionalContext ) );
			  CypherAdapterStream stream = new CypherAdapterStream( result, clock );

			  // When
			  MapValue meta = MetadataOf( stream );

			  // Then
			  assertThat( meta.Get( "type" ), equalTo( stringValue( "rw" ) ) );
			  assertThat( meta.Get( "stats" ), equalTo( MapValues( "nodes-created", intValue( 1 ), "nodes-deleted", intValue( 2 ), "relationships-created", intValue( 3 ), "relationships-deleted", intValue( 4 ), "properties-set", intValue( 5 ), "indexes-added", intValue( 6 ), "indexes-removed", intValue( 7 ), "constraints-added", intValue( 8 ), "constraints-removed", intValue( 9 ), "labels-added", intValue( 10 ), "labels-removed", intValue( 11 ) ) ) );
			  assertThat( meta.Get( "result_consumed_after" ), equalTo( longValue( 1337L ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIncludePlanIfPresent() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIncludePlanIfPresent()
		 {
			  // Given
			  QueryStatistics queryStatistics = mock( typeof( QueryStatistics ) );
			  when( queryStatistics.ContainsUpdates() ).thenReturn(false);
			  QueryResult result = mock( typeof( QueryResult ) );
			  when( result.FieldNames() ).thenReturn(new string[0]);
			  when( result.ExecutionType() ).thenReturn(explained(READ_ONLY));
			  when( result.QueryStatistics() ).thenReturn(queryStatistics);
			  when( result.Notifications ).thenReturn( Collections.emptyList() );
			  when( result.ExecutionPlanDescription() ).thenReturn(Plan("Join", map("arg1", 1), singletonList("id1"), Plan("Scan", map("arg2", 1), singletonList("id2"))));

			  TransactionalContext tc = mock( typeof( TransactionalContext ) );
			  CypherAdapterStream stream = new CypherAdapterStream( result, Clock.systemUTC() );

			  // When
			  MapValue meta = MetadataOf( stream );

			  // Then
			  MapValue expectedChild = MapValues( "args", MapValues( "arg2", intValue( 1 ) ), "identifiers", list( stringValue( "id2" ) ), "operatorType", stringValue( "Scan" ), "children", VirtualValues.EMPTY_LIST );
			  MapValue expectedPlan = MapValues( "args", MapValues( "arg1", intValue( 1 ) ), "identifiers", list( stringValue( "id1" ) ), "operatorType", stringValue( "Join" ), "children", list( expectedChild ) );
			  assertThat( meta.Get( "plan" ), equalTo( expectedPlan ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIncludeProfileIfPresent() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIncludeProfileIfPresent()
		 {
			  // Given
			  QueryStatistics queryStatistics = mock( typeof( QueryStatistics ) );
			  when( queryStatistics.ContainsUpdates() ).thenReturn(false);
			  QueryResult result = mock( typeof( QueryResult ) );
			  when( result.FieldNames() ).thenReturn(new string[0]);
			  when( result.ExecutionType() ).thenReturn(explained(READ_ONLY));
			  when( result.QueryStatistics() ).thenReturn(queryStatistics);
			  when( result.Notifications ).thenReturn( Collections.emptyList() );
			  when( result.ExecutionPlanDescription() ).thenReturn(Plan("Join", map("arg1", 1), 2, 4, 3, 1, singletonList("id1"), Plan("Scan", map("arg2", 1), 2, 4, 7, 1, singletonList("id2"))));

			  TransactionalContext tc = mock( typeof( TransactionalContext ) );
			  CypherAdapterStream stream = new CypherAdapterStream( result, Clock.systemUTC() );

			  // When
			  MapValue meta = MetadataOf( stream );

			  // Then
			  MapValue expectedChild = MapValues( "args", MapValues( "arg2", intValue( 1 ) ), "identifiers", list( stringValue( "id2" ) ), "operatorType", stringValue( "Scan" ), "children", VirtualValues.EMPTY_LIST, "rows", longValue( 1L ), "dbHits", longValue( 2L ), "pageCacheHits", longValue( 4L ), "pageCacheMisses", longValue( 7L ), "pageCacheHitRatio", doubleValue( 4.0 / 11 ) );

			  MapValue expectedProfile = MapValues( "args", MapValues( "arg1", intValue( 1 ) ), "identifiers", list( stringValue( "id1" ) ), "operatorType", stringValue( "Join" ), "children", list( expectedChild ), "rows", longValue( 1L ), "dbHits", longValue( 2L ), "pageCacheHits", longValue( 4L ), "pageCacheMisses", longValue( 3L ), "pageCacheHitRatio", doubleValue( 4.0 / 7 ) );

			  AssertMapEqualsWithDelta( ( MapValue ) meta.Get( "profile" ), expectedProfile, 0.0001 );
		 }

		 private MapValue MapValues( params object[] values )
		 {
			  int i = 0;
			  MapValueBuilder builder = new MapValueBuilder();
			  while ( i < values.Length )
			  {
					builder.Add( ( string ) values[i++], ( AnyValue ) values[i++] );
			  }
			  return builder.Build();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIncludeNotificationsIfPresent() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldIncludeNotificationsIfPresent()
		 {
			  // Given
			  QueryResult result = mock( typeof( QueryResult ) );
			  when( result.FieldNames() ).thenReturn(new string[0]);

			  QueryStatistics queryStatistics = mock( typeof( QueryStatistics ) );
			  when( queryStatistics.ContainsUpdates() ).thenReturn(false);

			  when( result.QueryStatistics() ).thenReturn(queryStatistics);
			  when( result.ExecutionType() ).thenReturn(query(READ_WRITE));

			  when( result.Notifications ).thenReturn( Arrays.asList( NotificationCode.INDEX_HINT_UNFULFILLABLE.notification( InputPosition.empty ), NotificationCode.PLANNER_UNSUPPORTED.notification( new InputPosition( 4, 5, 6 ) ) ) );
			  TransactionalContext tc = mock( typeof( TransactionalContext ) );
			  CypherAdapterStream stream = new CypherAdapterStream( result, Clock.systemUTC() );

			  // When
			  MapValue meta = MetadataOf( stream );

			  // Then
			  MapValue msg1 = MapValues( "severity", stringValue( "WARNING" ), "code", stringValue( "Neo.ClientError.Schema.IndexNotFound" ), "title", stringValue( "The request (directly or indirectly) referred to an index that does not exist." ), "description", stringValue( "The hinted index does not exist, please check the schema" ) );
			  MapValue msg2 = MapValues( "severity", stringValue( "WARNING" ), "code", stringValue( "Neo.ClientNotification.Statement.PlannerUnsupportedWarning" ), "title", stringValue( "This query is not supported by the COST planner." ), "description", stringValue( "Using COST planner is unsupported for this query, please use RULE planner instead" ), "position", MapValues( "offset", intValue( 4 ), "column", intValue( 6 ), "line", intValue( 5 ) ) );

			  assertThat( meta.Get( "notifications" ), equalTo( list( msg1, msg2 ) ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.values.virtual.MapValue metadataOf(CypherAdapterStream stream) throws Exception
		 private MapValue MetadataOf( CypherAdapterStream stream )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.values.virtual.MapValueBuilder meta = new org.neo4j.values.virtual.MapValueBuilder();
			  MapValueBuilder meta = new MapValueBuilder();
			  stream.Accept( new BoltResult_VisitorAnonymousInnerClass( this, meta ) );
			  return meta.Build();
		 }

		 private class BoltResult_VisitorAnonymousInnerClass : Neo4Net.Bolt.runtime.BoltResult_Visitor
		 {
			 private readonly CypherAdapterStreamTest _outerInstance;

			 private MapValueBuilder _meta;

			 public BoltResult_VisitorAnonymousInnerClass( CypherAdapterStreamTest outerInstance, MapValueBuilder meta )
			 {
				 this.outerInstance = outerInstance;
				 this._meta = meta;
			 }

			 public void visit( Neo4Net.Cypher.result.QueryResult_Record record )
			 {

			 }

			 public void addMetadata( string key, AnyValue value )
			 {
				  _meta.add( key, value );
			 }
		 }

		 private static void AssertMapEqualsWithDelta( MapValue a, MapValue b, double delta )
		 {
			  assertThat( "Map should have same size", a.Size(), equalTo(b.Size()) );
			  a.Foreach((key, value) =>
			  {
			  AnyValue aValue = value;
			  AnyValue bValue = b.Get( key );
			  if ( aValue is MapValue )
			  {
				  assertThat( "Value mismatch", bValue is MapValue );
				  AssertMapEqualsWithDelta( ( MapValue ) aValue, ( MapValue ) bValue, delta );
			  }
			  else if ( aValue is DoubleValue )
			  {
				  assertThat( "Value mismatch", ( ( DoubleValue ) aValue ).doubleValue(), closeTo(((DoubleValue) bValue).doubleValue(), delta) );
			  }
			  else
			  {
				  assertThat( "Value mismatch", aValue, equalTo( bValue ) );
			  }
			  });
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static org.neo4j.graphdb.ExecutionPlanDescription plan(final String name, final java.util.Map<String,Object> args, final long dbHits, final long pageCacheHits, final long pageCacheMisses, final long rows, final java.util.List<String> identifiers, final org.neo4j.graphdb.ExecutionPlanDescription... children)
		 private static ExecutionPlanDescription Plan( string name, IDictionary<string, object> args, long dbHits, long pageCacheHits, long pageCacheMisses, long rows, IList<string> identifiers, params ExecutionPlanDescription[] children )
		 {
			  return plan(name, args, identifiers, new ExecutionPlanDescription_ProfilerStatisticsAnonymousInnerClass(dbHits, pageCacheHits, pageCacheMisses, rows)
			 , children);
		 }

		 private class ExecutionPlanDescription_ProfilerStatisticsAnonymousInnerClass : Neo4Net.Graphdb.ExecutionPlanDescription_ProfilerStatistics
		 {
			 private long _dbHits;
			 private long _pageCacheHits;
			 private long _pageCacheMisses;
			 private long _rows;

			 public ExecutionPlanDescription_ProfilerStatisticsAnonymousInnerClass( long dbHits, long pageCacheHits, long pageCacheMisses, long rows )
			 {
				 this._dbHits = dbHits;
				 this._pageCacheHits = pageCacheHits;
				 this._pageCacheMisses = pageCacheMisses;
				 this._rows = rows;
			 }

			 public long Rows
			 {
				 get
				 {
					  return _rows;
				 }
			 }

			 public long DbHits
			 {
				 get
				 {
					  return _dbHits;
				 }
			 }

			 public long PageCacheHits
			 {
				 get
				 {
					  return _pageCacheHits;
				 }
			 }

			 public long PageCacheMisses
			 {
				 get
				 {
					  return _pageCacheMisses;
				 }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static org.neo4j.graphdb.ExecutionPlanDescription plan(final String name, final java.util.Map<String,Object> args, final java.util.List<String> identifiers, final org.neo4j.graphdb.ExecutionPlanDescription... children)
		 private static ExecutionPlanDescription Plan( string name, IDictionary<string, object> args, IList<string> identifiers, params ExecutionPlanDescription[] children )
		 {
			  return Plan( name, args, identifiers, null, children );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static org.neo4j.graphdb.ExecutionPlanDescription plan(final String name, final java.util.Map<String,Object> args, final java.util.List<String> identifiers, final org.neo4j.graphdb.ExecutionPlanDescription_ProfilerStatistics profile, final org.neo4j.graphdb.ExecutionPlanDescription... children)
		 private static ExecutionPlanDescription Plan( string name, IDictionary<string, object> args, IList<string> identifiers, Neo4Net.Graphdb.ExecutionPlanDescription_ProfilerStatistics profile, params ExecutionPlanDescription[] children )
		 {
			  return new ExecutionPlanDescriptionAnonymousInnerClass( name, args, identifiers, profile, children );
		 }

		 private class ExecutionPlanDescriptionAnonymousInnerClass : ExecutionPlanDescription
		 {
			 private string _name;
			 private IDictionary<string, object> _args;
			 private IList<string> _identifiers;
			 private Neo4Net.Graphdb.ExecutionPlanDescription_ProfilerStatistics _profile;
			 private ExecutionPlanDescription[] _children;

			 public ExecutionPlanDescriptionAnonymousInnerClass( string name, IDictionary<string, object> args, IList<string> identifiers, Neo4Net.Graphdb.ExecutionPlanDescription_ProfilerStatistics profile, ExecutionPlanDescription[] children )
			 {
				 this._name = name;
				 this._args = args;
				 this._identifiers = identifiers;
				 this._profile = profile;
				 this._children = children;
			 }

			 public string Name
			 {
				 get
				 {
					  return _name;
				 }
			 }

			 public IList<ExecutionPlanDescription> Children
			 {
				 get
				 {
					  return new IList<ExecutionPlanDescription> { _children };
				 }
			 }

			 public IDictionary<string, object> Arguments
			 {
				 get
				 {
					  return _args;
				 }
			 }

			 public ISet<string> Identifiers
			 {
				 get
				 {
					  return new HashSet<string>( _identifiers );
				 }
			 }

			 public bool hasProfilerStatistics()
			 {
				  return _profile != null;
			 }

			 public Neo4Net.Graphdb.ExecutionPlanDescription_ProfilerStatistics ProfilerStatistics
			 {
				 get
				 {
					  return _profile;
				 }
			 }
		 }
	}

}