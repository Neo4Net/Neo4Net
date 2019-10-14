using System.Collections.Generic;
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

	public abstract class TokenRecord : AbstractBaseRecord
	{
		 private int _nameId;
		 private IList<DynamicRecord> _nameRecords;

		 public TokenRecord( long id ) : base( id )
		 {
		 }

		 public virtual TokenRecord Initialize( bool inUse, int nameId )
		 {
			  base.Initialize( inUse );
			  this._nameId = nameId;
			  this._nameRecords = new List<DynamicRecord>();
			  return this;
		 }

		 public override void Clear()
		 {
			  Initialize( false, Record.NoNextBlock.intValue() );
		 }

		 public virtual bool Light
		 {
			 get
			 {
				  return _nameRecords == null || _nameRecords.Count == 0;
			 }
		 }

		 public virtual int NameId
		 {
			 get
			 {
				  return _nameId;
			 }
			 set
			 {
				  this._nameId = value;
			 }
		 }


		 public virtual ICollection<DynamicRecord> NameRecords
		 {
			 get
			 {
				  return _nameRecords;
			 }
		 }

		 public virtual void AddNameRecord( DynamicRecord record )
		 {
			  _nameRecords.Add( record );
		 }

		 public virtual void AddNameRecords( IEnumerable<DynamicRecord> records )
		 {
			  foreach ( DynamicRecord record in records )
			  {
					AddNameRecord( record );
			  }
		 }

		 public override string ToString()
		 {
			  StringBuilder buf = new StringBuilder( SimpleName() + '[' );
			  buf.Append( Id ).Append( ',' ).Append( InUse() ? "in" : "no" ).Append(" use");
			  buf.Append( ",nameId=" ).Append( _nameId );
			  AdditionalToString( buf );
			  if ( !Light )
			  {
					foreach ( DynamicRecord dyn in _nameRecords )
					{
						 buf.Append( ',' ).Append( dyn );
					}
			  }
			  return buf.Append( ']' ).ToString();
		 }

		 protected internal abstract string SimpleName();

		 protected internal virtual void AdditionalToString( StringBuilder buf )
		 {
			  // default: nothing additional
		 }
	}

}