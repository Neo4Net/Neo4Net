﻿/*
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
namespace Neo4Net.Kernel.Impl.Store.Records
{
	public class RelationshipTypeTokenRecord : TokenRecord
	{
		 public RelationshipTypeTokenRecord( int id ) : base( id )
		 {
		 }

		 public override RelationshipTypeTokenRecord Initialize( bool inUse, int nameId )
		 {
			  base.Initialize( inUse, nameId );
			  return this;
		 }

		 protected internal override string SimpleName()
		 {
			  return "RelationshipType";
		 }

		 public override RelationshipTypeTokenRecord Clone()
		 {
			  RelationshipTypeTokenRecord relationshipTypeTokenRecord = new RelationshipTypeTokenRecord( IntId );
			  relationshipTypeTokenRecord.InUse = InUse();
			  if ( Created )
			  {
					relationshipTypeTokenRecord.SetCreated();
			  }
			  relationshipTypeTokenRecord.NameId = NameId;
			  relationshipTypeTokenRecord.AddNameRecords( NameRecords );
			  return relationshipTypeTokenRecord;
		 }
	}

}