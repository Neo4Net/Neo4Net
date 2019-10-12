using System.Collections.Generic;

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
namespace Org.Neo4j.Kernel.impl.transaction.state
{

	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using Org.Neo4j.Kernel.impl.store;
	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;
	using TokenRecord = Org.Neo4j.Kernel.impl.store.record.TokenRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.PropertyStore.encodeString;

	public class TokenCreator<R> where R : Org.Neo4j.Kernel.impl.store.record.TokenRecord
	{
		 private readonly TokenStore<R> _store;

		 public TokenCreator( TokenStore<R> store )
		 {
			  this._store = store;
		 }

		 public virtual void CreateToken( string name, long id, RecordAccess<R, Void> recordAccess )
		 {
			  R record = recordAccess.Create( id, null ).forChangingData();
			  record.InUse = true;
			  record.setCreated();
			  ICollection<DynamicRecord> nameRecords = _store.allocateNameRecords( encodeString( name ) );
			  record.NameId = ( int ) Iterables.first( nameRecords ).Id;
			  record.addNameRecords( nameRecords );
		 }
	}

}