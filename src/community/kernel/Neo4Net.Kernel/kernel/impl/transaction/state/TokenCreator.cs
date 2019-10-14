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
namespace Neo4Net.Kernel.impl.transaction.state
{

	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using Neo4Net.Kernel.impl.store;
	using DynamicRecord = Neo4Net.Kernel.Impl.Store.Records.DynamicRecord;
	using TokenRecord = Neo4Net.Kernel.Impl.Store.Records.TokenRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.PropertyStore.encodeString;

	public class TokenCreator<R> where R : Neo4Net.Kernel.Impl.Store.Records.TokenRecord
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