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


	public class StubNodeLabelIndexCursor : NodeLabelIndexCursor
	{
		 private int _offset = -1;
		 private readonly IDictionary<int, long[]> _lookup;
		 private int _label;

		 public StubNodeLabelIndexCursor( IDictionary<int, long[]> lookup )
		 {
			  this._lookup = lookup;
		 }

		 internal virtual void Initialize( int label )
		 {
			  this._label = label;
			  this._offset = -1;
		 }

		 public override void Node( NodeCursor cursor )
		 {

		 }

		 public override long NodeReference()
		 {
			  long[] nodes = _lookup[_label];
			  if ( nodes == null )
			  {
					return -1L;
			  }

			  return _offset >= 0 && _offset < nodes.Length ? nodes[_offset] : -1;
		 }

		 public override LabelSet Labels()
		 {
			  return null;
		 }

		 public override bool Next()
		 {
			  long[] nodes = _lookup[_label];
			  return nodes != null && ++_offset < nodes.Length;
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