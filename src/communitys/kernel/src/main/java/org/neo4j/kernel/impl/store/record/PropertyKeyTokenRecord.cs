using System.Text;

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
namespace Neo4Net.Kernel.impl.store.record
{
	public class PropertyKeyTokenRecord : TokenRecord
	{
		 private int _propCount;

		 public PropertyKeyTokenRecord( int id ) : base( id )
		 {
		 }

		 public virtual PropertyKeyTokenRecord Initialize( bool inUse, int nameId, int propertyCount )
		 {
			  base.Initialize( inUse, nameId );
			  this._propCount = propertyCount;
			  return this;
		 }

		 public override void Clear()
		 {
			  base.Clear();
			  _propCount = 0;
		 }

		 protected internal override string SimpleName()
		 {
			  return "PropertyKey";
		 }

		 public virtual int PropertyCount
		 {
			 get
			 {
				  return _propCount;
			 }
			 set
			 {
				  this._propCount = value;
			 }
		 }


		 protected internal override void AdditionalToString( StringBuilder buf )
		 {
			  buf.Append( ",propCount=" ).Append( _propCount );
		 }

		 public override PropertyKeyTokenRecord Clone()
		 {
			  PropertyKeyTokenRecord propertyKeyTokenRecord = new PropertyKeyTokenRecord( IntId );
			  propertyKeyTokenRecord.InUse = InUse();
			  if ( Created )
			  {
					propertyKeyTokenRecord.SetCreated();
			  }
			  propertyKeyTokenRecord.NameId = NameId;
			  propertyKeyTokenRecord.AddNameRecords( NameRecords );
			  propertyKeyTokenRecord.PropertyCount = PropertyCount;
			  return propertyKeyTokenRecord;
		 }
	}

}