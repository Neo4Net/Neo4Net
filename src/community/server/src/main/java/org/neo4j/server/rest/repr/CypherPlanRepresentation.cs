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

	using ExecutionPlanDescription = Neo4Net.GraphDb.ExecutionPlanDescription;
	using Neo4Net.Collections.Helpers;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.server.rest.repr.ObjectToRepresentationConverter.getMapRepresentation;

	/// <summary>
	/// This takes a function that resolves to a <seealso cref="org.Neo4Net.graphdb.ExecutionPlanDescription"/>, and it does so for two reasons:
	///  - The plan description needs to be fetched *after* the result is streamed to the user
	///  - This method is recursive, so it's not enough to just pass in the execution plan to the root call of it
	///    subsequent inner calls could not re-use that execution plan (that would just lead to an infinite loop)
	/// </summary>
	public abstract class CypherPlanRepresentation : MappingRepresentation
	{

		 private CypherPlanRepresentation() : base("plan")
		 {
		 }

		 protected internal abstract ExecutionPlanDescription Plan { get; }

		 protected internal override void Serialize( MappingSerializer mappingSerializer )
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.graphdb.ExecutionPlanDescription planDescription = getPlan();
			  ExecutionPlanDescription planDescription = Plan;

			  mappingSerializer.PutString( "name", planDescription.Name );
			  IDictionary<string, object> arguments = planDescription.Arguments;
			  MappingRepresentation argsRepresentation = getMapRepresentation( arguments );
			  mappingSerializer.PutMapping( "args", argsRepresentation );

			  if ( planDescription.HasProfilerStatistics() )
			  {
					Neo4Net.GraphDb.ExecutionPlanDescription_ProfilerStatistics stats = planDescription.ProfilerStatistics;
					mappingSerializer.PutNumber( "rows", stats.Rows );
					mappingSerializer.PutNumber( "dbHits", stats.DbHits );
			  }

			  mappingSerializer.PutList( "children", new ListRepresentation( "children", new IterableWrapperAnonymousInnerClass( this, planDescription.Children ) ) );
		 }

		 private class IterableWrapperAnonymousInnerClass : IterableWrapper<Representation, ExecutionPlanDescription>
		 {
			 private readonly CypherPlanRepresentation _outerInstance;

			 public IterableWrapperAnonymousInnerClass( CypherPlanRepresentation outerInstance, IList<ExecutionPlanDescription> getChildren ) : base( getChildren )
			 {
				 this.outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: protected Representation underlyingObjectToObject(final org.Neo4Net.graphdb.ExecutionPlanDescription childPlan)
			 protected internal override Representation underlyingObjectToObject( ExecutionPlanDescription childPlan )
			 {
				  return NewFromPlan( childPlan );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static CypherPlanRepresentation newFromProvider(final System.Func<Object, org.Neo4Net.graphdb.ExecutionPlanDescription> planProvider)
		 public static CypherPlanRepresentation NewFromProvider( System.Func<object, ExecutionPlanDescription> planProvider )
		 {
			  return new CypherPlanRepresentationAnonymousInnerClass( planProvider );
		 }

		 private class CypherPlanRepresentationAnonymousInnerClass : CypherPlanRepresentation
		 {
			 private System.Func<object, ExecutionPlanDescription> _planProvider;

			 public CypherPlanRepresentationAnonymousInnerClass( System.Func<object, ExecutionPlanDescription> planProvider )
			 {
				 this._planProvider = planProvider;
			 }

			 private ExecutionPlanDescription plan;
			 private bool fetched;

			 protected internal override ExecutionPlanDescription Plan
			 {
				 get
				 {
					  if ( !fetched )
					  {
							plan = _planProvider( null );
							fetched = true;
					  }
					  return plan;
				 }
			 }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static CypherPlanRepresentation newFromPlan(final org.Neo4Net.graphdb.ExecutionPlanDescription plan)
		 public static CypherPlanRepresentation NewFromPlan( ExecutionPlanDescription plan )
		 {
			  return new CypherPlanRepresentationAnonymousInnerClass2( plan );
		 }

		 private class CypherPlanRepresentationAnonymousInnerClass2 : CypherPlanRepresentation
		 {
			 private ExecutionPlanDescription _plan;

			 public CypherPlanRepresentationAnonymousInnerClass2( ExecutionPlanDescription plan )
			 {
				 this._plan = plan;
			 }

			 protected internal override ExecutionPlanDescription Plan
			 {
				 get
				 {
					  return _plan;
				 }
			 }
		 }
	}

}