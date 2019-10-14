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
namespace Neo4Net.Internal.Kernel.Api.helpers
{

	using Neo4Net.Helpers.Collections;
	using Value = Neo4Net.Values.Storable.Value;

	public class StubNodeValueIndexCursor : NodeValueIndexCursor
	{
		 private IEnumerator<Pair<long, Value[]>> _things;
		 private Pair<long, Value[]> _current;

		 public StubNodeValueIndexCursor( IEnumerator<Pair<long, Value[]>> things )
		 {
			  this._things = things;
		 }

		 public override void Node( NodeCursor cursor )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override long NodeReference()
		 {
			  return _current.first();
		 }

		 public override bool Next()
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  if ( _things.hasNext() )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					_current = _things.next();
					return true;
			  }
			  return false;
		 }

		 public override int NumberOfProperties()
		 {
			  return _current.other().Length;
		 }

		 public override int PropertyKey( int offset )
		 {
			  return 0;
		 }

		 public override bool HasValue()
		 {
			  return _current.other() != null;
		 }

		 public override Value PropertyValue( int offset )
		 {
			  return _current.other()[offset];
		 }

		 public override void Close()
		 {

		 }

		 public virtual bool Closed
		 {
			 get
			 {
				  return false;
			 }
		 }
	}

}