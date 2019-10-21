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
namespace Neo4Net.Kernel.Impl.Index.Schema
{

	using Neo4Net.Cursors;
	using Neo4Net.Index.Internal.gbptree;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;

	internal class ResultCursor : IRawCursor<Hit<StringIndexKey, NativeIndexValue>, IOException>
	{
		 private readonly IEnumerator<string> _iterator;
		 private string _current;
		 private int _pos = -1;

		 internal ResultCursor( IEnumerator<string> keys )
		 {
			  _iterator = keys;
		 }

		 public override bool Next()
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  if ( _iterator.hasNext() )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					_current = _iterator.next();
					_pos++;
					return true;
			  }
			  return false;
		 }

		 public override void Close()
		 {
			  // do nothing
		 }

		 public override Hit<StringIndexKey, NativeIndexValue> Get()
		 {
			  StringIndexKey key = new StringIndexKey();
			  key.Initialize( _pos );
			  key.From( stringValue( _current ) );
			  return new SimpleHit<StringIndexKey, NativeIndexValue>( key, NativeIndexValue.Instance );
		 }
	}

}