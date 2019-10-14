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
namespace Neo4Net.Server.rest.repr
{

	using ExecutionPlanDescription = Neo4Net.Graphdb.ExecutionPlanDescription;
	using Node = Neo4Net.Graphdb.Node;
	using Path = Neo4Net.Graphdb.Path;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using Result = Neo4Net.Graphdb.Result;
	using Neo4Net.Helpers.Collections;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.loop;

	public class CypherResultRepresentation : MappingRepresentation
	{
		 private static readonly RepresentationDispatcher _representationDispatcher = new CypherRepresentationDispatcher();

		 private readonly ListRepresentation _resultRepresentation;
		 private readonly ListRepresentation _columns;
		 private readonly MappingRepresentation _statsRepresentation;
		 private readonly MappingRepresentation _plan;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public CypherResultRepresentation(final org.neo4j.graphdb.Result result, boolean includeStats, boolean includePlan)
		 public CypherResultRepresentation( Result result, bool includeStats, bool includePlan ) : base( RepresentationType.String )
		 {
			  _resultRepresentation = CreateResultRepresentation( result );
			  _columns = ListRepresentation.String( result.Columns() );
			  _statsRepresentation = includeStats ? new CypherStatisticsRepresentation( result.QueryStatistics ) : null;
			  _plan = includePlan ? CypherPlanRepresentation.NewFromProvider( PlanProvider( result ) ) : null;
		 }

		 protected internal override void Serialize( MappingSerializer serializer )
		 {
			  serializer.PutList( "columns", _columns );
			  serializer.PutList( "data", _resultRepresentation );

			  if ( _statsRepresentation != null )
			  {
					serializer.PutMapping( "stats", _statsRepresentation );
			  }
			  if ( _plan != null )
			  {
					serializer.PutMapping( "plan", _plan );
			  }
		 }

		 private ListRepresentation CreateResultRepresentation( Result executionResult )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<String> columns = executionResult.columns();
			  IList<string> columns = executionResult.Columns();
			  IEnumerable<IDictionary<string, object>> inner = new RepresentationExceptionHandlingIterable<IDictionary<string, object>>( loop( executionResult ) );
			  return new ListRepresentation( "data", new IterableWrapperAnonymousInnerClass( this, inner, columns ) );
		 }

		 private class IterableWrapperAnonymousInnerClass : IterableWrapper<Representation, IDictionary<string, object>>
		 {
			 private readonly CypherResultRepresentation _outerInstance;

			 private IList<string> _columns;

			 public IterableWrapperAnonymousInnerClass( CypherResultRepresentation outerInstance, IEnumerable<IDictionary<string, object>> inner, IList<string> columns ) : base( inner )
			 {
				 this.outerInstance = outerInstance;
				 this._columns = columns;
			 }


//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected Representation underlyingObjectToObject(final java.util.Map<String,Object> row)
			 protected internal override Representation underlyingObjectToObject( IDictionary<string, object> row )
			 {
				  return new ListRepresentation( "row", new IterableWrapperAnonymousInnerClass2( this, _columns, row ) );
			 }

			 private class IterableWrapperAnonymousInnerClass2 : IterableWrapper<Representation, string>
			 {
				 private readonly IterableWrapperAnonymousInnerClass _outerInstance;

				 private IDictionary<string, object> _row;

				 public IterableWrapperAnonymousInnerClass2( IterableWrapperAnonymousInnerClass outerInstance, IList<string> columns, IDictionary<string, object> row ) : base( columns )
				 {
					 this.outerInstance = outerInstance;
					 this._row = row;
				 }


				 protected internal override Representation underlyingObjectToObject( string column )
				 {
					  return outerInstance.outerInstance.getRepresentation( _row[column] );
				 }
			 }
		 }

		 private Representation GetRepresentation( object r )
		 {
			  if ( r == null )
			  {
					return ValueRepresentation.String( null );
			  }

			  if ( r is Path )
			  {
					return new PathRepresentation<>( ( Path ) r );
			  }

			  if ( r is System.Collections.IEnumerable )
			  {
					return HandleIterable( ( System.Collections.IEnumerable ) r );
			  }

			  if ( r is Node )
			  {
					return new NodeRepresentation( ( Node ) r );
			  }

			  if ( r is Relationship )
			  {
					return new RelationshipRepresentation( ( Relationship ) r );
			  }

			  return _representationDispatcher.dispatch( r, "" );
		 }

		 private Representation HandleIterable( System.Collections.IEnumerable data )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<Representation> results = new java.util.ArrayList<>();
			  IList<Representation> results = new List<Representation>();
			  foreach ( Object value in data )
			  {
					Representation rep = GetRepresentation( value );
					results.Add( rep );
			  }

			  RepresentationType representationType = GetType( results );
			  return new ListRepresentation( representationType, results );
		 }

		 private RepresentationType GetType( IList<Representation> representations )
		 {
			  if ( representations == null || representations.Count == 0 )
			  {
					return RepresentationType.String;
			  }
			  return representations[0].RepresentationType;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private System.Func<Object, org.neo4j.graphdb.ExecutionPlanDescription> planProvider(final org.neo4j.graphdb.Result result)
		 private System.Func<object, ExecutionPlanDescription> PlanProvider( Result result )
		 {
			  return from => result.ExecutionPlanDescription;
		 }

	}

}