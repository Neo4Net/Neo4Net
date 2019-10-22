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
namespace Neo4Net.GraphDb.impl.notification
{

	using Status = Neo4Net.Kernel.Api.Exceptions.Status;

	/// <summary>
	/// Notification codes are status codes identifying the type of notification.
	/// </summary>
	public sealed class NotificationCode
	{
		 public static readonly NotificationCode CartesianProduct = new NotificationCode( "CartesianProduct", InnerEnum.CartesianProduct, Neo4Net.GraphDb.SeverityLevel.Warning, Neo4Net.Kernel.Api.Exceptions.Status_Statement.CartesianProductWarning, "If a part of a query contains multiple disconnected patterns, this will build a " + "cartesian product between all those parts. This may produce a large amount of data and slow down" + " query processing. " + "While occasionally intended, it may often be possible to reformulate the query that avoids the " + "use of this cross " + "product, perhaps by adding a relationship between the different parts or by using OPTIONAL MATCH" );
		 public static readonly NotificationCode LegacyPlanner = new NotificationCode( "LegacyPlanner", InnerEnum.LegacyPlanner, Neo4Net.GraphDb.SeverityLevel.Warning, Neo4Net.Kernel.Api.Exceptions.Status_Statement.FeatureDeprecationWarning, "Using PLANNER for switching between planners has been deprecated, please use CYPHER planner=[rule,cost] instead" );
		 public static readonly NotificationCode DeprecatedRulePlanner = new NotificationCode( "DeprecatedRulePlanner", InnerEnum.DeprecatedRulePlanner, Neo4Net.GraphDb.SeverityLevel.Warning, Neo4Net.Kernel.Api.Exceptions.Status_Statement.FeatureDeprecationWarning, "The rule planner, which was used to plan this query, is deprecated and will be discontinued soon. " + "If you did not explicitly choose the rule planner, you should try to change your query so that the " + "rule planner is not used" );
		 public static readonly NotificationCode DeprecatedCompiledRuntime = new NotificationCode( "DeprecatedCompiledRuntime", InnerEnum.DeprecatedCompiledRuntime, Neo4Net.GraphDb.SeverityLevel.Warning, Neo4Net.Kernel.Api.Exceptions.Status_Statement.FeatureDeprecationWarning, "The compiled runtime, which was requested to execute this query, is deprecated and will be removed in a future release." );
		 public static readonly NotificationCode PlannerUnsupported = new NotificationCode( "PlannerUnsupported", InnerEnum.PlannerUnsupported, Neo4Net.GraphDb.SeverityLevel.Warning, Neo4Net.Kernel.Api.Exceptions.Status_Statement.PlannerUnsupportedWarning, "Using COST planner is unsupported for this query, please use RULE planner instead" );
		 public static readonly NotificationCode RulePlannerUnavailableFallback = new NotificationCode( "RulePlannerUnavailableFallback", InnerEnum.RulePlannerUnavailableFallback, Neo4Net.GraphDb.SeverityLevel.Warning, Neo4Net.Kernel.Api.Exceptions.Status_Statement.PlannerUnavailableWarning, "Using RULE planner is unsupported for current CYPHER version, the query has been executed by an older CYPHER " + "version" );
		 public static readonly NotificationCode RuntimeUnsupported = new NotificationCode( "RuntimeUnsupported", InnerEnum.RuntimeUnsupported, Neo4Net.GraphDb.SeverityLevel.Warning, Neo4Net.Kernel.Api.Exceptions.Status_Statement.RuntimeUnsupportedWarning, "Selected runtime is unsupported for this query, please use a different runtime instead or fallback to default." );
		 public static readonly NotificationCode IndexHintUnfulfillable = new NotificationCode( "IndexHintUnfulfillable", InnerEnum.IndexHintUnfulfillable, Neo4Net.GraphDb.SeverityLevel.Warning, Neo4Net.Kernel.Api.Exceptions.Status_Schema.IndexNotFound, "The hinted index does not exist, please check the schema" );
		 public static readonly NotificationCode JoinHintUnfulfillable = new NotificationCode( "JoinHintUnfulfillable", InnerEnum.JoinHintUnfulfillable, Neo4Net.GraphDb.SeverityLevel.Warning, Neo4Net.Kernel.Api.Exceptions.Status_Statement.JoinHintUnfulfillableWarning, "The hinted join was not planned. This could happen because no generated plan contained the join key, " + "please try using a different join key or restructure your query." );
		 public static readonly NotificationCode JoinHintUnsupported = new NotificationCode( "JoinHintUnsupported", InnerEnum.JoinHintUnsupported, Neo4Net.GraphDb.SeverityLevel.Warning, Neo4Net.Kernel.Api.Exceptions.Status_Statement.JoinHintUnsupportedWarning, "Using RULE planner is unsupported for queries with join hints, please use COST planner instead" );
		 public static readonly NotificationCode LengthOnNonPath = new NotificationCode( "LengthOnNonPath", InnerEnum.LengthOnNonPath, Neo4Net.GraphDb.SeverityLevel.Warning, Neo4Net.Kernel.Api.Exceptions.Status_Statement.FeatureDeprecationWarning, "Using 'length' on anything that is not a path is deprecated, please use 'size' instead" );
		 public static readonly NotificationCode IndexLookupForDynamicProperty = new NotificationCode( "IndexLookupForDynamicProperty", InnerEnum.IndexLookupForDynamicProperty, Neo4Net.GraphDb.SeverityLevel.Warning, Neo4Net.Kernel.Api.Exceptions.Status_Statement.DynamicPropertyWarning, "Using a dynamic property makes it impossible to use an index lookup for this query" );
		 public static readonly NotificationCode BareNodeSyntaxDeprecated = new NotificationCode( "BareNodeSyntaxDeprecated", InnerEnum.BareNodeSyntaxDeprecated, Neo4Net.GraphDb.SeverityLevel.Warning, Neo4Net.Kernel.Api.Exceptions.Status_Statement.FeatureDeprecationWarning, "Use of bare node patterns has been deprecated. Please enclose the identifier in parenthesis." );
		 public static readonly NotificationCode DeprecatedFunction = new NotificationCode( "DeprecatedFunction", InnerEnum.DeprecatedFunction, Neo4Net.GraphDb.SeverityLevel.Warning, Neo4Net.Kernel.Api.Exceptions.Status_Statement.FeatureDeprecationWarning, "The query used a deprecated function." );
		 public static readonly NotificationCode DeprecatedProcedure = new NotificationCode( "DeprecatedProcedure", InnerEnum.DeprecatedProcedure, Neo4Net.GraphDb.SeverityLevel.Warning, Neo4Net.Kernel.Api.Exceptions.Status_Statement.FeatureDeprecationWarning, "The query used a deprecated procedure." );
		 public static readonly NotificationCode ProcedureWarning = new NotificationCode( "ProcedureWarning", InnerEnum.ProcedureWarning, Neo4Net.GraphDb.SeverityLevel.Warning, Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureWarning, "The query used a procedure that generated a warning." );
		 public static readonly NotificationCode DeprecatedProcedureReturnField = new NotificationCode( "DeprecatedProcedureReturnField", InnerEnum.DeprecatedProcedureReturnField, Neo4Net.GraphDb.SeverityLevel.Warning, Neo4Net.Kernel.Api.Exceptions.Status_Statement.FeatureDeprecationWarning, "The query used a deprecated field from a procedure." );
		 public static readonly NotificationCode DeprecatedBindingVarLengthRelationship = new NotificationCode( "DeprecatedBindingVarLengthRelationship", InnerEnum.DeprecatedBindingVarLengthRelationship, Neo4Net.GraphDb.SeverityLevel.Warning, Neo4Net.Kernel.Api.Exceptions.Status_Statement.FeatureDeprecationWarning, "Binding relationships to a list in a variable length pattern is deprecated." );
		 public static readonly NotificationCode DeprecatedRelationshipTypeSeparator = new NotificationCode( "DeprecatedRelationshipTypeSeparator", InnerEnum.DeprecatedRelationshipTypeSeparator, Neo4Net.GraphDb.SeverityLevel.Warning, Neo4Net.Kernel.Api.Exceptions.Status_Statement.FeatureDeprecationWarning, "The semantics of using colon in the separation of alternative relationship types in conjunction with the " + "use of variable binding, inlined property predicates, or variable length will change in a future version." );
		 public static readonly NotificationCode EagerLoadCsv = new NotificationCode( "EagerLoadCsv", InnerEnum.EagerLoadCsv, Neo4Net.GraphDb.SeverityLevel.Warning, Neo4Net.Kernel.Api.Exceptions.Status_Statement.EagerOperatorWarning, "Using LOAD CSV with a large data set in a query where the execution plan contains the " + "Eager operator could potentially consume a lot of memory and is likely to not perform well. " + "See the Neo4Net Manual entry on the Eager operator for more information and hints on " + "how problems could be avoided." );
		 public static readonly NotificationCode LargeLabelLoadCsv = new NotificationCode( "LargeLabelLoadCsv", InnerEnum.LargeLabelLoadCsv, Neo4Net.GraphDb.SeverityLevel.Warning, Neo4Net.Kernel.Api.Exceptions.Status_Statement.NoApplicableIndexWarning, "Using LOAD CSV followed by a MATCH or MERGE that matches a non-indexed label will most likely " + "not perform well on large data sets. Please consider using a schema index." );
		 public static readonly NotificationCode MissingLabel = new NotificationCode( "MissingLabel", InnerEnum.MissingLabel, Neo4Net.GraphDb.SeverityLevel.Warning, Neo4Net.Kernel.Api.Exceptions.Status_Statement.UnknownLabelWarning, "One of the labels in your query is not available in the database, make sure you didn't " + "misspell it or that the label is available when you run this statement in your application" );
		 public static readonly NotificationCode MissingRelType = new NotificationCode( "MissingRelType", InnerEnum.MissingRelType, Neo4Net.GraphDb.SeverityLevel.Warning, Neo4Net.Kernel.Api.Exceptions.Status_Statement.UnknownRelationshipTypeWarning, "One of the relationship types in your query is not available in the database, make sure you didn't " + "misspell it or that the label is available when you run this statement in your application" );
		 public static readonly NotificationCode MissingPropertyName = new NotificationCode( "MissingPropertyName", InnerEnum.MissingPropertyName, Neo4Net.GraphDb.SeverityLevel.Warning, Neo4Net.Kernel.Api.Exceptions.Status_Statement.UnknownPropertyKeyWarning, "One of the property names in your query is not available in the database, make sure you didn't " + "misspell it or that the label is available when you run this statement in your application" );
		 public static readonly NotificationCode UnboundedShortestPath = new NotificationCode( "UnboundedShortestPath", InnerEnum.UnboundedShortestPath, Neo4Net.GraphDb.SeverityLevel.Warning, Neo4Net.Kernel.Api.Exceptions.Status_Statement.UnboundedVariableLengthPatternWarning, "Using shortest path with an unbounded pattern will likely result in long execution times. " + "It is recommended to use an upper limit to the number of node hops in your pattern." );
		 public static readonly NotificationCode ExhaustiveShortestPath = new NotificationCode( "ExhaustiveShortestPath", InnerEnum.ExhaustiveShortestPath, Neo4Net.GraphDb.SeverityLevel.Warning, Neo4Net.Kernel.Api.Exceptions.Status_Statement.ExhaustiveShortestPathWarning, "Using shortest path with an exhaustive search fallback might cause query slow down since shortest path " + "graph algorithms might not work for this use case. It is recommended to introduce a WITH to separate the " + "MATCH containing the shortest path from the existential predicates on that path." );
		 public static readonly NotificationCode CreateUniqueUnavailableFallback = new NotificationCode( "CreateUniqueUnavailableFallback", InnerEnum.CreateUniqueUnavailableFallback, Neo4Net.GraphDb.SeverityLevel.Warning, Neo4Net.Kernel.Api.Exceptions.Status_Statement.PlannerUnavailableWarning, "CREATE UNIQUE is unsupported for current CYPHER version, the query has been executed by an older CYPHER version" );
		 public static readonly NotificationCode CreateUniqueDeprecated = new NotificationCode( "CreateUniqueDeprecated", InnerEnum.CreateUniqueDeprecated, Neo4Net.GraphDb.SeverityLevel.Warning, Neo4Net.Kernel.Api.Exceptions.Status_Statement.FeatureDeprecationWarning, "CREATE UNIQUE is deprecated and will be removed in a future version." );
		 public static readonly NotificationCode StartUnavailableFallback = new NotificationCode( "StartUnavailableFallback", InnerEnum.StartUnavailableFallback, Neo4Net.GraphDb.SeverityLevel.Warning, Neo4Net.Kernel.Api.Exceptions.Status_Statement.PlannerUnavailableWarning, "START is not supported for current CYPHER version, the query has been executed by an older CYPHER version" );
		 public static readonly NotificationCode StartDeprecated = new NotificationCode( "StartDeprecated", InnerEnum.StartDeprecated, Neo4Net.GraphDb.SeverityLevel.Warning, Neo4Net.Kernel.Api.Exceptions.Status_Statement.FeatureDeprecationWarning, "START has been deprecated and will be removed in a future version." );
		 public static readonly NotificationCode ExperimentalFeature = new NotificationCode( "ExperimentalFeature", InnerEnum.ExperimentalFeature, Neo4Net.GraphDb.SeverityLevel.Warning, Neo4Net.Kernel.Api.Exceptions.Status_Statement.ExperimentalFeature, "You are using an experimental feature" );
		 public static readonly NotificationCode SuboptimalIndexForContainsQuery = new NotificationCode( "SuboptimalIndexForContainsQuery", InnerEnum.SuboptimalIndexForContainsQuery, Neo4Net.GraphDb.SeverityLevel.Information, Neo4Net.Kernel.Api.Exceptions.Status_Statement.SuboptimalIndexForWildcardQuery, "If the performance of this statement using `CONTAINS` doesn't meet your expectations check out the alternative index-providers, see " + "documentation on index configuration." );
		 public static readonly NotificationCode SuboptimalIndexForEndsWithQuery = new NotificationCode( "SuboptimalIndexForEndsWithQuery", InnerEnum.SuboptimalIndexForEndsWithQuery, Neo4Net.GraphDb.SeverityLevel.Information, Neo4Net.Kernel.Api.Exceptions.Status_Statement.SuboptimalIndexForWildcardQuery, "If the performance of this statement using `ENDS WITH` doesn't meet your expectations check out the alternative index-providers, see " + "documentation on index configuration." );

		 private static readonly IList<NotificationCode> valueList = new List<NotificationCode>();

		 static NotificationCode()
		 {
			 valueList.Add( CartesianProduct );
			 valueList.Add( LegacyPlanner );
			 valueList.Add( DeprecatedRulePlanner );
			 valueList.Add( DeprecatedCompiledRuntime );
			 valueList.Add( PlannerUnsupported );
			 valueList.Add( RulePlannerUnavailableFallback );
			 valueList.Add( RuntimeUnsupported );
			 valueList.Add( IndexHintUnfulfillable );
			 valueList.Add( JoinHintUnfulfillable );
			 valueList.Add( JoinHintUnsupported );
			 valueList.Add( LengthOnNonPath );
			 valueList.Add( IndexLookupForDynamicProperty );
			 valueList.Add( BareNodeSyntaxDeprecated );
			 valueList.Add( DeprecatedFunction );
			 valueList.Add( DeprecatedProcedure );
			 valueList.Add( ProcedureWarning );
			 valueList.Add( DeprecatedProcedureReturnField );
			 valueList.Add( DeprecatedBindingVarLengthRelationship );
			 valueList.Add( DeprecatedRelationshipTypeSeparator );
			 valueList.Add( EagerLoadCsv );
			 valueList.Add( LargeLabelLoadCsv );
			 valueList.Add( MissingLabel );
			 valueList.Add( MissingRelType );
			 valueList.Add( MissingPropertyName );
			 valueList.Add( UnboundedShortestPath );
			 valueList.Add( ExhaustiveShortestPath );
			 valueList.Add( CreateUniqueUnavailableFallback );
			 valueList.Add( CreateUniqueDeprecated );
			 valueList.Add( StartUnavailableFallback );
			 valueList.Add( StartDeprecated );
			 valueList.Add( ExperimentalFeature );
			 valueList.Add( SuboptimalIndexForContainsQuery );
			 valueList.Add( SuboptimalIndexForEndsWithQuery );
		 }

		 public enum InnerEnum
		 {
			 CartesianProduct,
			 LegacyPlanner,
			 DeprecatedRulePlanner,
			 DeprecatedCompiledRuntime,
			 PlannerUnsupported,
			 RulePlannerUnavailableFallback,
			 RuntimeUnsupported,
			 IndexHintUnfulfillable,
			 JoinHintUnfulfillable,
			 JoinHintUnsupported,
			 LengthOnNonPath,
			 IndexLookupForDynamicProperty,
			 BareNodeSyntaxDeprecated,
			 DeprecatedFunction,
			 DeprecatedProcedure,
			 ProcedureWarning,
			 DeprecatedProcedureReturnField,
			 DeprecatedBindingVarLengthRelationship,
			 DeprecatedRelationshipTypeSeparator,
			 EagerLoadCsv,
			 LargeLabelLoadCsv,
			 MissingLabel,
			 MissingRelType,
			 MissingPropertyName,
			 UnboundedShortestPath,
			 ExhaustiveShortestPath,
			 CreateUniqueUnavailableFallback,
			 CreateUniqueDeprecated,
			 StartUnavailableFallback,
			 StartDeprecated,
			 ExperimentalFeature,
			 SuboptimalIndexForContainsQuery,
			 SuboptimalIndexForEndsWithQuery
		 }

		 public readonly InnerEnum innerEnumValue;
		 private readonly string nameValue;
		 private readonly int ordinalValue;
		 private static int nextOrdinal = 0;

		 internal Private readonly;
		 internal Private readonly;
		 internal Private readonly;

		 internal NotificationCode( string name, InnerEnum innerEnum, Neo4Net.GraphDb.SeverityLevel severity, Neo4Net.Kernel.Api.Exceptions.Status status, string description )
		 {
			  this._severity = severity;
			  this._status = status;
			  this._description = description;

			 nameValue = name;
			 ordinalValue = nextOrdinal++;
			 innerEnumValue = innerEnum;
		 }

		 // TODO: Move construction of Notifications to a factory with explicit methods per type of notification
		 public Notification Notification( Neo4Net.GraphDb.InputPosition position, params NotificationDetail[] details )
		 {
			  return new Notification( position, details );
		 }

//JAVA TO C# CONVERTER TODO TASK: Java to C# Converter does not convert types within enums:
//		 public final class Notification implements org.Neo4Net.graphdb.Notification
	//	 {
	//		  private final InputPosition position;
	//		  private final String detailedDescription;
	//
	//		  Notification(InputPosition position, NotificationDetail... details)
	//		  {
	//				this.position = position;
	//
	//				if (details.length == 0)
	//				{
	//					 this.detailedDescription = description;
	//				}
	//				else
	//				{
	//					 StringBuilder builder = new StringBuilder(description.length());
	//					 builder.append(description);
	//					 builder.append(' ');
	//					 builder.append('(');
	//					 String comma = "";
	//					 for (NotificationDetail detail : details)
	//					 {
	//						  builder.append(comma);
	//						  builder.append(detail);
	//						  comma = ", ";
	//					 }
	//					 builder.append(')');
	//
	//					 this.detailedDescription = builder.toString();
	//				}
	//		  }
	//
	//		  @@Override public String getCode()
	//		  {
	//				return status.code().serialize();
	//		  }
	//
	//		  @@Override public String getTitle()
	//		  {
	//				return status.code().description();
	//		  }
	//
	//		  @@Override public String getDescription()
	//		  {
	//				return detailedDescription;
	//		  }
	//
	//		  @@Override public InputPosition getPosition()
	//		  {
	//				return position;
	//		  }
	//
	//		  @@Override public SeverityLevel getSeverity()
	//		  {
	//				return severity;
	//		  }
	//
	//		  @@Override public String toString()
	//		  {
	//				return "Notification{" +
	//						  "position=" + position +
	//						  ", detailedDescription='" + detailedDescription + '\'' +
	//						  '}';
	//		  }
	//
	//		  @@Override public boolean equals(Object o)
	//		  {
	//				if (this == o)
	//				{
	//					 return true;
	//				}
	//				if (o == null || getClass() != o.getClass())
	//				{
	//					 return false;
	//				}
	//				Notification that = (Notification) o;
	//				return Objects.equals(position, that.position) && Objects.equals(detailedDescription, that.detailedDescription);
	//		  }
	//
	//		  @@Override public int hashCode()
	//		  {
	//				return Objects.hash(position, detailedDescription);
	//		  }
	//	 }

		public static IList<NotificationCode> values()
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

		public static NotificationCode valueOf( string name )
		{
			foreach ( NotificationCode enumInstance in NotificationCode.valueList )
			{
				if ( enumInstance.nameValue == name )
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException( name );
		}
	}

}