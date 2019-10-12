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
namespace Neo4Net.Kernel.impl.store.record
{
	public class LabelTokenRecord : TokenRecord
	{
		 public LabelTokenRecord( long id ) : base( id )
		 {
		 }

		 public override LabelTokenRecord Initialize( bool inUse, int nameId )
		 {
			  base.Initialize( inUse, nameId );
			  return this;
		 }

		 protected internal override string SimpleName()
		 {
			  return "Label";
		 }

		 public override LabelTokenRecord Clone()
		 {
			  LabelTokenRecord labelTokenRecord = new LabelTokenRecord( IntId );
			  labelTokenRecord.InUse = InUse();
			  if ( Created )
			  {
					labelTokenRecord.SetCreated();
			  }
			  labelTokenRecord.NameId = NameId;
			  labelTokenRecord.AddNameRecords( NameRecords );
			  return labelTokenRecord;
		 }
	}

}