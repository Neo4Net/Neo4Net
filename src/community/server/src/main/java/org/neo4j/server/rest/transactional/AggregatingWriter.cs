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
namespace Neo4Net.Server.rest.transactional
{
	using JsonGenerator = org.codehaus.jackson.JsonGenerator;

	using Result = Neo4Net.GraphDb.Result;

	internal class AggregatingWriter : ResultDataContentWriter
	{
		 private readonly ResultDataContentWriter[] _writers;

		 internal AggregatingWriter( ResultDataContentWriter[] writers )
		 {
			  this._writers = writers;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void write(org.codehaus.jackson.JsonGenerator out, Iterable<String> columns, Neo4Net.graphdb.Result_ResultRow row, TransactionStateChecker txStateChecker) throws java.io.IOException
		 public override void Write( JsonGenerator @out, IEnumerable<string> columns, Neo4Net.GraphDb.Result_ResultRow row, TransactionStateChecker txStateChecker )
		 {
			  foreach ( ResultDataContentWriter writer in _writers )
			  {
					writer.Write( @out, columns, row, txStateChecker );
			  }
		 }
	}

}