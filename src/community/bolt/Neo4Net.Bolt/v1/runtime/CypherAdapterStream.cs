using System.Collections.Generic;

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
namespace Neo4Net.Bolt.v1.runtime
{

	using BoltResult = Neo4Net.Bolt.runtime.BoltResult;
	using QueryResult = Neo4Net.Cypher.result.QueryResult;
	using ExecutionPlanDescription = Neo4Net.GraphDb.ExecutionPlanDescription;
	using InputPosition = Neo4Net.GraphDb.InputPosition;
	using Notification = Neo4Net.GraphDb.Notification;
	using QueryExecutionType = Neo4Net.GraphDb.QueryExecutionType;
	using QueryStatistics = Neo4Net.GraphDb.QueryStatistics;
	using AnyValue = Neo4Net.Values.AnyValue;
	using Values = Neo4Net.Values.Storable.Values;
	using MapValue = Neo4Net.Values.@virtual.MapValue;
	using MapValueBuilder = Neo4Net.Values.@virtual.MapValueBuilder;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.intValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.longValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.stringValue;

	public class CypherAdapterStream : BoltResult
	{
		 private readonly QueryResult @delegate;
		 private readonly string[] _fieldNames;
		 private readonly Clock _clock;

		 public CypherAdapterStream( QueryResult @delegate, Clock clock )
		 {
			  this.@delegate = @delegate;
			  this._fieldNames = @delegate.FieldNames();
			  this._clock = clock;
		 }

		 public override void Close()
		 {
			  @delegate.Close();
		 }

		 public override string[] FieldNames()
		 {
			  return _fieldNames;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void accept(final Neo4Net.bolt.runtime.BoltResult_Visitor visitor) throws Exception
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public override void Accept( Neo4Net.Bolt.runtime.BoltResult_Visitor visitor )
		 {
			  long start = _clock.millis();
			  @delegate.Accept(row =>
			  {
				visitor.Visit( row );
				return true;
			  });
			  AddRecordStreamingTime( visitor, _clock.millis() - start );
			  QueryExecutionType qt = @delegate.ExecutionType();
			  visitor.AddMetadata( "type", Values.stringValue( QueryTypeCode( qt.QueryType() ) ) );

			  if ( @delegate.QueryStatistics().containsUpdates() )
			  {
					MapValue stats = QueryStats( @delegate.QueryStatistics() );
					visitor.AddMetadata( "stats", stats );
			  }
			  if ( qt.RequestedExecutionPlanDescription() )
			  {
					ExecutionPlanDescription rootPlanTreeNode = @delegate.ExecutionPlanDescription();
					string metadataFieldName = rootPlanTreeNode.HasProfilerStatistics() ? "profile" : "plan";
					visitor.AddMetadata( metadataFieldName, ExecutionPlanConverter.Convert( rootPlanTreeNode ) );
			  }

			  IEnumerable<Notification> notifications = @delegate.Notifications;
			  if ( notifications.GetEnumerator().hasNext() )
			  {
					visitor.AddMetadata( "notifications", NotificationConverter.Convert( notifications ) );
			  }
		 }

		 protected internal virtual void AddRecordStreamingTime( Neo4Net.Bolt.runtime.BoltResult_Visitor visitor, long time )
		 {
			  visitor.AddMetadata( "result_consumed_after", longValue( time ) );
		 }

		 public override string ToString()
		 {
			  return "CypherAdapterStream{" + "delegate=" + @delegate + ", fieldNames=" + Arrays.ToString( _fieldNames ) + '}';
		 }

		 private MapValue QueryStats( QueryStatistics queryStatistics )
		 {
			  MapValueBuilder builder = new MapValueBuilder();
			  AddIfNonZero( builder, "nodes-created", queryStatistics.NodesCreated );
			  AddIfNonZero( builder, "nodes-deleted", queryStatistics.NodesDeleted );
			  AddIfNonZero( builder, "relationships-created", queryStatistics.RelationshipsCreated );
			  AddIfNonZero( builder, "relationships-deleted", queryStatistics.RelationshipsDeleted );
			  AddIfNonZero( builder, "properties-set", queryStatistics.PropertiesSet );
			  AddIfNonZero( builder, "labels-added", queryStatistics.LabelsAdded );
			  AddIfNonZero( builder, "labels-removed", queryStatistics.LabelsRemoved );
			  AddIfNonZero( builder, "indexes-added", queryStatistics.IndexesAdded );
			  AddIfNonZero( builder, "indexes-removed", queryStatistics.IndexesRemoved );
			  AddIfNonZero( builder, "constraints-added", queryStatistics.ConstraintsAdded );
			  AddIfNonZero( builder, "constraints-removed", queryStatistics.ConstraintsRemoved );
			  return builder.Build();
		 }

		 private void AddIfNonZero( MapValueBuilder builder, string name, int count )
		 {
			  if ( count > 0 )
			  {
					builder.Add( name, intValue( count ) );
			  }
		 }

		 private string QueryTypeCode( QueryExecutionType.QueryType queryType )
		 {
			  switch ( queryType.innerEnumValue )
			  {
			  case QueryExecutionType.QueryType.InnerEnum.READ_ONLY:
					return "r";

			  case QueryExecutionType.QueryType.InnerEnum.READ_WRITE:
					return "rw";

			  case QueryExecutionType.QueryType.InnerEnum.WRITE:
					return "w";

			  case QueryExecutionType.QueryType.InnerEnum.SCHEMA_WRITE:
					return "s";

			  default:
					return queryType.name();
			  }
		 }

		 private class NotificationConverter
		 {
			  public static AnyValue Convert( IEnumerable<Notification> notifications )
			  {
					IList<AnyValue> @out = new List<AnyValue>();
					foreach ( Notification notification in notifications )
					{
						 InputPosition pos = notification.Position; // position is optional
						 bool includePosition = !pos.Equals( InputPosition.empty );
						 int size = includePosition ? 5 : 4;
						 MapValueBuilder builder = new MapValueBuilder( size );

						 builder.Add( "code", stringValue( notification.Code ) );
						 builder.Add( "title", stringValue( notification.Title ) );
						 builder.Add( "description", stringValue( notification.Description ) );
						 builder.Add( "severity", stringValue( notification.Severity.ToString() ) );

						 if ( includePosition )
						 {
							  // only add the position if it is not empty
							  builder.Add( "position", VirtualValues.map( new string[]{ "offset", "line", "column" }, new AnyValue[]{ intValue( pos.Offset ), intValue( pos.Line ), intValue( pos.Column ) } ) );
						 }

						 @out.Add( builder.Build() );
					}
					return VirtualValues.fromList( @out );
			  }
		 }
	}

}