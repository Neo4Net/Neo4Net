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
namespace Org.Neo4j.Cypher.@internal.runtime.compiled.expressions
{
	using CypherTypeException = Org.Neo4j.Cypher.@internal.v3_5.util.CypherTypeException;

	using ExecutionContext = Org.Neo4j.Cypher.@internal.runtime.interpreted.ExecutionContext;
	using NotFoundException = Org.Neo4j.Graphdb.NotFoundException;
	using AnyValue = Org.Neo4j.Values.AnyValue;
	using BooleanValue = Org.Neo4j.Values.Storable.BooleanValue;
	using Value = Org.Neo4j.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.NO_VALUE;

	/// <summary>
	/// Contains helper methods used from compiled expressions
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") public final class CompiledHelpers
	public sealed class CompiledHelpers
	{
		 private CompiledHelpers()
		 {
			  throw new System.NotSupportedException( "do not instantiate" );
		 }

		 public static Value AssertBooleanOrNoValue( AnyValue value )
		 {
			  if ( value != NO_VALUE && !( value is BooleanValue ) )
			  {
					throw new CypherTypeException( string.Format( "Don't know how to treat a predicate: {0}", value.ToString() ), null );
			  }
			  return ( Value ) value;
		 }

		 public static AnyValue LoadVariable( ExecutionContext ctx, string name )
		 {
			  if ( !ctx.contains( name ) )
			  {
					throw new NotFoundException( string.Format( "Unknown variable `{0}`.", name ) );
			  }
			  return ctx.apply( name );
		 }
	}

}