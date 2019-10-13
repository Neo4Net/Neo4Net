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
namespace Neo4Net.Kernel.impl.store.record
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.Record.NULL_REFERENCE;

	public class SchemaRecord : AbstractBaseRecord, IEnumerable<DynamicRecord>
	{
		 private ICollection<DynamicRecord> _records;

		 public SchemaRecord( ICollection<DynamicRecord> records ) : base( -1 )
		 {
			  Initialize( records );
		 }

		 public virtual SchemaRecord Initialize( ICollection<DynamicRecord> records )
		 {
			  Initialize( true );
			  IEnumerator<DynamicRecord> iterator = records.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  long id = iterator.hasNext() ? iterator.next().Id : NULL_REFERENCE.intValue();
			  Id = id;
			  this._records = records;
			  return this;
		 }

		 public virtual ICollection<DynamicRecord> DynamicRecords
		 {
			 set
			 {
				  this._records.Clear();
				  this._records.addAll( value );
			 }
		 }

		 public override void Clear()
		 {
			  base.Clear();
			  this._records = null;
		 }

		 public override IEnumerator<DynamicRecord> Iterator()
		 {
			  return _records.GetEnumerator();
		 }

		 public virtual int Size()
		 {
			  return _records.Count;
		 }

		 public override SchemaRecord Clone()
		 {
			  IList<DynamicRecord> list = new List<DynamicRecord>( _records.Count );
			  foreach ( DynamicRecord record in _records )
			  {
					list.Add( record.Clone() );
			  }
			  return new SchemaRecord( list );
		 }
	}

}