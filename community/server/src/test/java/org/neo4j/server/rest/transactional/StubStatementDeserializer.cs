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

	using Neo4jError = Org.Neo4j.Server.rest.transactional.error.Neo4jError;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.iterator;

	public class StubStatementDeserializer : StatementDeserializer
	{
		 private readonly IEnumerator<Statement> _statements;
		 private readonly IEnumerator<Neo4jError> _errors;

		 private bool _hasNext;
		 private Statement _next;

		 public static StubStatementDeserializer Statements( params Statement[] statements )
		 {
			  return new StubStatementDeserializer( emptyIterator(), iterator(statements) );
		 }

		 public StubStatementDeserializer( IEnumerator<Neo4jError> errors, IEnumerator<Statement> statements ) : base( new System.IO.MemoryStream_Input( new sbyte[]{} ) )
		 {
			  this._statements = statements;
			  this._errors = errors;

			  ComputeNext();
		 }

		 private void ComputeNext()
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  _hasNext = _statements.hasNext();
			  if ( _hasNext )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					_next = _statements.next();
			  }
			  else
			  {
					_next = null;
			  }
		 }

		 public override bool HasNext()
		 {
			  return _hasNext;
		 }

		 public override Statement Peek()
		 {
			  if ( _hasNext )
			  {
					return _next;
			  }
			  else
			  {
					throw new NoSuchElementException();
			  }
		 }

		 public override Statement Next()
		 {
			  Statement result = _next;
			  ComputeNext();
			  return result;
		 }

		 public override IEnumerator<Neo4jError> Errors()
		 {
			  return _errors;
		 }
	}

}