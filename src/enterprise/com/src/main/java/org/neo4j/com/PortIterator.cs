using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.com
{

	public class PortIterator : IEnumerator<int>
	{
		 private readonly int _start;
		 private readonly int _end;
		 private int _next;

		 public PortIterator( int[] portRanges )
		 {
			  _start = portRanges[0];
			  _end = portRanges[1];
			  _next = _start;
		 }

		 public override bool HasNext()
		 {
			  return _start < _end ? _next <= _end : _next >= _end;
		 }

		 public override int? Next()
		 {
			  if ( !HasNext() )
			  {
					throw new NoSuchElementException();
			  }
			  return _start < _end ? _next++: _next--;
		 }

		 public override void Remove()
		 {
			  throw new System.NotSupportedException();
		 }
	}

}