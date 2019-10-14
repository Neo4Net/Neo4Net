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
namespace Neo4Net.Kernel.impl.storemigration.participant
{

	using RecordStorageReader = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageReader;
	using StorageEntityCursor = Neo4Net.Storageengine.Api.StorageEntityCursor;
	using StoragePropertyCursor = Neo4Net.Storageengine.Api.StoragePropertyCursor;
	using InputChunk = Neo4Net.@unsafe.Impl.Batchimport.input.InputChunk;
	using InputEntityVisitor = Neo4Net.@unsafe.Impl.Batchimport.input.InputEntityVisitor;

	internal abstract class StoreScanChunk<T> : InputChunk where T : Neo4Net.Storageengine.Api.StorageEntityCursor
	{
		 protected internal readonly StoragePropertyCursor StorePropertyCursor;
		 protected internal readonly T Cursor;
		 private readonly bool _requiresPropertyMigration;
		 private long _id;
		 private long _endId;

		 internal StoreScanChunk( T cursor, RecordStorageReader storageReader, bool requiresPropertyMigration )
		 {
			  this.Cursor = cursor;
			  this._requiresPropertyMigration = requiresPropertyMigration;
			  this.StorePropertyCursor = storageReader.AllocatePropertyCursor();
		 }

		 internal virtual void VisitProperties( T record, InputEntityVisitor visitor )
		 {
			  if ( !_requiresPropertyMigration )
			  {
					visitor.PropertyId( record.propertiesReference() );
			  }
			  else
			  {
					StorePropertyCursor.init( record.propertiesReference() );
					while ( StorePropertyCursor.next() )
					{
						 // add key as int here as to have the importer use the token id
						 visitor.Property( StorePropertyCursor.propertyKey(), StorePropertyCursor.propertyValue().asObject() );
					}
					StorePropertyCursor.close();
			  }
		 }

		 public override void Close()
		 {
			  StorePropertyCursor.close();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean next(org.neo4j.unsafe.impl.batchimport.input.InputEntityVisitor visitor) throws java.io.IOException
		 public override bool Next( InputEntityVisitor visitor )
		 {
			  if ( _id < _endId )
			  {
					Read( Cursor, _id );
					if ( Cursor.next() )
					{
						 VisitRecord( Cursor, visitor );
						 visitor.EndOfEntity();
					}
					_id++;
					return true;
			  }
			  return false;
		 }

		 protected internal abstract void Read( T cursor, long id );

		 public virtual void Initialize( long startId, long endId )
		 {
			  this._id = startId;
			  this._endId = endId;
		 }

		 internal abstract void VisitRecord( T record, InputEntityVisitor visitor );
	}

}