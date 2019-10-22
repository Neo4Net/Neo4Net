/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Cypher.Internal.runtime.compiled.expressions
{
	using CypherTypeException = Neo4Net.Cypher.Internal.v3_5.util.CypherTypeException;

	using ExecutionContext = Neo4Net.Cypher.Internal.runtime.interpreted.ExecutionContext;
	using NotFoundException = Neo4Net.GraphDb.NotFoundException;
	using AnyValue = Neo4Net.Values.AnyValue;
	using BooleanValue = Neo4Net.Values.Storable.BooleanValue;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.values.storable.Values.NO_VALUE;

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