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
namespace Neo4Net.GraphDb
{

	/// <summary>
	/// Signifies how a query is executed, as well as what side effects and results could be expected from the query.
	/// <para>
	/// In Cypher there are three different modes of execution:
	/// <ul>
	/// <li>Normal execution,</li>
	/// <li>execution with the <a href="https://Neo4Net.com/docs/developer-manual/current/cypher/execution-plans/">{@code PROFILE}</a> directive,
	/// and</li>
	/// <li>execution with the <a href="https://Neo4Net.com/docs/developer-manual/current/cypher/execution-plans/">{@code EXPLAIN}</a>
	/// directive.</li>
	/// </ul>
	/// Instances of this class contain the required information to be able to tell these different execution modes apart.
	/// It also contains information about what effects the query could have, and whether it could yield any results, in
	/// form
	/// of the <seealso cref="QueryType QueryType enum"/>.
	/// </para>
	/// <para>
	/// Queries executed with the {@code PROFILE} directive can have side effects and produce results in the same way as a
	/// normally executed method. The difference being that the user has expressed an interest in seeing the plan used to
	/// execute the query, and that this plan will (after execution completes) be annotated with
	/// <seealso cref="IExecutionPlanDescription.getProfilerStatistics() profiling information"/> from the execution of the query.
	/// </para>
	/// <para>
	/// Queries executed with the {@code EXPLAIN} directive never have any side effects, nor do they ever yield any rows in
	/// the results, the sole purpose of this mode of execution is to
	/// <seealso cref="IResult.getExecutionPlanDescription() get a description of the plan"/> that <i>would</i> be executed
	/// if/when the query is executed normally (or under {@code PROFILE}).
	/// </para>
	/// </summary>
	public sealed class QueryExecutionType
	{
		 /// <summary>
		 /// Signifies what type of query an <seealso cref="QueryExecutionType"/> executes.
		 /// </summary>
		 public sealed class QueryType
		 {
			  /// <summary>
			  /// A read-only query, that does not change any data, but only produces a result. </summary>
			  public static readonly QueryType ReadOnly = new QueryType( "ReadOnly", InnerEnum.ReadOnly );
			  /// <summary>
			  /// A read/write query, that creates or updates data, and also produces a result. </summary>
			  public static readonly QueryType ReadWrite = new QueryType( "ReadWrite", InnerEnum.ReadWrite );
			  /// <summary>
			  /// A write-only query, that creates or updates data, but does not yield any rows in the result. </summary>
			  public static readonly QueryType Write = new QueryType( "Write", InnerEnum.Write );
			  /// <summary>
			  /// A schema changing query, that updates the schema but neither changes any data nor yields any rows in the
			  /// result.
			  /// </summary>
			  public static readonly QueryType SchemaWrite = new QueryType( "SchemaWrite", InnerEnum.SchemaWrite );
			  public static readonly QueryType  = new QueryType( "", InnerEnum. );

			  private static readonly IList<QueryType> valueList = new List<QueryType>();

			  static QueryType()
			  {
				  valueList.Add( ReadOnly );
				  valueList.Add( ReadWrite );
				  valueList.Add( Write );
				  valueList.Add( SchemaWrite );
				  valueList.Add();
			  }

			  public enum InnerEnum
			  {
				  ReadOnly,
				  ReadWrite,
				  Write,
				  SchemaWrite,
              
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private QueryType( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }
			  internal readonly QueryExecutionType query;
			  internal readonly QueryExecutionType profiled;
			  internal readonly QueryExecutionType explained;

			  internal QueryType( string name, InnerEnum innerEnum )
			  {
					this._query = new QueryExecutionType( Execution.Query, this );
					this._profiled = new QueryExecutionType( Execution.Profile, this );
					this._explained = new QueryExecutionType( Execution.Explain, this );

				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			 public static IList<QueryType> values()
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

			 public static QueryType valueOf( string name )
			 {
				 foreach ( QueryType enumInstance in QueryType.valueList )
				 {
					 if ( enumInstance.nameValue == name )
					 {
						 return enumInstance;
					 }
				 }
				 throw new System.ArgumentException( name );
			 }
		 }

		 /// <summary>
		 /// Get the <seealso cref="QueryExecutionType"/> that signifies normal execution of a query of the supplied type.
		 /// </summary>
		 /// <param name="type"> the type of query executed. </param>
		 /// <returns> The instance that signifies normal execution of the supplied <seealso cref="QueryType"/>. </returns>
		 public static QueryExecutionType Query( QueryType type )
		 {
			  return requireNonNull( type, "QueryType" ).query;
		 }

		 /// <summary>
		 /// Get the <seealso cref="QueryExecutionType"/> that signifies profiled execution of a query of the supplied type.
		 /// </summary>
		 /// <param name="type"> the type of query executed. </param>
		 /// <returns> The instance that signifies profiled execution of the supplied <seealso cref="QueryType"/>. </returns>
		 public static QueryExecutionType Profiled( QueryType type )
		 {
			  return requireNonNull( type, "QueryType" ).profiled;
		 }

		 /// <summary>
		 /// Get the <seealso cref="QueryExecutionType"/> that signifies explaining the plan of a query of the supplied type.
		 /// </summary>
		 /// <param name="type"> the type of query executed. </param>
		 /// <returns> The instance that signifies explaining the plan of the supplied <seealso cref="QueryType"/>. </returns>
		 public static QueryExecutionType Explained( QueryType type )
		 {
			  return requireNonNull( type, "QueryType" ).explained;
		 }

		 /// <summary>
		 /// Get the type of query this execution refers to.
		 /// </summary>
		 /// <returns> the type of query this execution refers to. </returns>
		 public QueryType QueryType()
		 {
			  return _type;
		 }

		 /// <summary>
		 /// Signifies whether results from this execution
		 /// <seealso cref="IExecutionPlanDescription.getProfilerStatistics() contains profiling information"/>.
		 /// 
		 /// This is {@code true} for queries executed with the
		 /// <a href="https://Neo4Net.com/docs/developer-manual/current/cypher/execution-plans/">{@code PROFILE}</a> directive.
		 /// </summary>
		 /// <returns> {@code true} if the results from this execution would contain profiling information. </returns>
		 public bool Profiled
		 {
			 get
			 {
				  return _execution == Execution.Profile;
			 }
		 }

		 /// <summary>
		 /// Signifies whether the supplied query contained a directive that asked for a
		 /// <seealso cref="IExecutionPlanDescription description of the execution plan"/>.
		 /// 
		 /// This is {@code true} for queries executed with either the
		 /// <a href="https://Neo4Net.com/docs/developer-manual/current/cypher/execution-plans/">{@code EXPLAIN} or {@code PROFILE} directives</a>.
		 /// </summary>
		 /// <returns> {@code true} if a description of the plan should be presented to the user. </returns>
		 public bool RequestedExecutionPlanDescription()
		 {
			  return _execution != Execution.Query;
		 }

		 /// <summary>
		 /// Signifies that the query was executed with the
		 /// <a href="https://Neo4Net.com/docs/developer-manual/current/cypher/execution-plans/">{@code EXPLAIN} directive</a>.
		 /// </summary>
		 /// <returns> {@code true} if the query was executed using the {@code EXPLAIN} directive. </returns>
		 public bool Explained
		 {
			 get
			 {
				  return _execution == Execution.Explain;
			 }
		 }

		 /// <summary>
		 /// Signifies that the execution of the query could produce a result.
		 /// 
		 /// This is an important distinction from the result being empty.
		 /// </summary>
		 /// <returns> {@code true} if the execution would yield rows in the result set. </returns>
		 public bool CanContainResults()
		 {
			  return ( _type == QueryType.ReadOnly || _type == QueryType.ReadWrite ) && _execution != Execution.Explain;
		 }

		 /// <summary>
		 /// Signifies that the execution of the query could perform changes to the data.
		 /// 
		 /// <seealso cref="IResult"/><seealso cref="IResult.getQueryStatistics() .getQueryStatistics()"/>{@link QueryStatistics#containsUpdates()
		 /// .containsUpdates()} signifies whether the query actually performed any updates.
		 /// </summary>
		 /// <returns> {@code true} if the execution could perform changes to data. </returns>
		 public bool CanUpdateData()
		 {
			  return ( _type == QueryType.ReadWrite || _type == QueryType.Write ) && _execution != Execution.Explain;
		 }

		 /// <summary>
		 /// Signifies that the execution of the query updates the schema.
		 /// </summary>
		 /// <returns> {@code true} if the execution updates the schema. </returns>
		 public bool CanUpdateSchema()
		 {
			  return _type == QueryType.SchemaWrite && _execution != Execution.Explain;
		 }

		 private readonly Execution _execution;
		 private readonly QueryType _type;

		 private QueryExecutionType( Execution execution, QueryType type )
		 {
			  this._execution = execution;
			  this._type = type;
		 }

		 public override string ToString()
		 {
			  return _execution.ToString( _type );
		 }

		 private sealed class Execution
		 {
//JAVA TO C# CONVERTER TODO TASK: Enum value-specific class bodies are not converted by Java to C# Converter:
//           QUERY { String toString(QueryType type) { return type.name(); } },
			  public static readonly Execution Profile = new Execution( "Profile", InnerEnum.Profile );
			  public static readonly Execution Explain = new Execution( "Explain", InnerEnum.Explain );
			  public static readonly Execution  = new Execution( "", InnerEnum. );

			  private static readonly IList<Execution> valueList = new List<Execution>();

			  static Execution()
			  {
				  valueList.Add( QUERY );
				  valueList.Add( Profile );
				  valueList.Add( Explain );
				  valueList.Add();
			  }

			  public enum InnerEnum
			  {
				  QUERY,
				  Profile,
				  Explain,
              
			  }

			  public readonly InnerEnum innerEnumValue;
			  private readonly string nameValue;
			  private readonly int ordinalValue;
			  private static int nextOrdinal = 0;

			  private Execution( string name, InnerEnum innerEnum )
			  {
				  nameValue = name;
				  ordinalValue = nextOrdinal++;
				  innerEnumValue = innerEnum;
			  }

			  internal string ToString( QueryType type )
			  {
					return name() + ":" + type.name();
			  }

			 public static IList<Execution> values()
			 {
				 return valueList;
			 }

			 public int ordinal()
			 {
				 return ordinalValue;
			 }

			 public static Execution valueOf( string name )
			 {
				 foreach ( Execution enumInstance in Execution.valueList )
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

}