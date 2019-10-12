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
namespace Org.Neo4j.Server.rest.transactional
{
	using JsonGenerator = org.codehaus.jackson.JsonGenerator;

	using Result = Org.Neo4j.Graphdb.Result;

	internal class RowWriter : ResultDataContentWriter
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(org.codehaus.jackson.JsonGenerator out, Iterable<String> columns, org.neo4j.graphdb.Result_ResultRow row, TransactionStateChecker txStateChecker) throws java.io.IOException
		 public override void Write( JsonGenerator @out, IEnumerable<string> columns, Org.Neo4j.Graphdb.Result_ResultRow row, TransactionStateChecker txStateChecker )
		 {
			  @out.writeArrayFieldStart( "row" );
			  try
			  {
					foreach ( string key in columns )
					{
						 @out.writeObject( row.Get( key ) );
					}
			  }
			  finally
			  {
					@out.writeEndArray();
					WriteMeta( @out, columns, row );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeMeta(org.codehaus.jackson.JsonGenerator out, Iterable<String> columns, org.neo4j.graphdb.Result_ResultRow row) throws java.io.IOException
		 private void WriteMeta( JsonGenerator @out, IEnumerable<string> columns, Org.Neo4j.Graphdb.Result_ResultRow row )
		 {
			  @out.writeArrayFieldStart( "meta" );
			  try
			  {
					/*
					 * The way we've designed this JSON serialization is by injecting a custom codec
					 * to write the entities. Unfortunately, there seems to be no way to control state
					 * inside the JsonGenerator, and so we need to make a second call to write out the
					 * meta information, directly to the injected codec. This is not very pretty,
					 * but time is expensive, and redesigning one of three server serialization
					 * formats is not a priority.
					 */
					Neo4jJsonCodec codec = ( Neo4jJsonCodec ) @out.Codec;
					foreach ( string key in columns )
					{
						 codec.WriteMeta( @out, row.Get( key ) );
					}
			  }
			  finally
			  {
					@out.writeEndArray();
			  }
		 }
	}

}