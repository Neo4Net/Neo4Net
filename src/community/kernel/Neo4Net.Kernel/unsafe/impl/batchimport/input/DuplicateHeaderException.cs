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
namespace Neo4Net.@unsafe.Impl.Batchimport.input
{
	using Header = Neo4Net.@unsafe.Impl.Batchimport.input.csv.Header;
	using Entry = Neo4Net.@unsafe.Impl.Batchimport.input.csv.Header.Entry;

	public class DuplicateHeaderException : HeaderException
	{
		 private readonly Header.Entry _first;
		 private readonly Header.Entry _other;

		 public DuplicateHeaderException( Header.Entry first, Header.Entry other ) : base( "Duplicate header entries found, first " + first + ", other (and conflicting) " + other )
		 {
			  this._first = first;
			  this._other = other;
		 }

		 public virtual Header.Entry First
		 {
			 get
			 {
				  return _first;
			 }
		 }

		 public virtual Header.Entry Other
		 {
			 get
			 {
				  return _other;
			 }
		 }
	}

}