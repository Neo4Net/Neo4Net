using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.discovery
{

	public class MockSrvRecordResolver : SrvRecordResolver
	{

		 private readonly Dictionary<string, IList<SrvRecord>> _records;

		 public MockSrvRecordResolver( Dictionary<string, IList<SrvRecord>> records )
		 {
			  this._records = records;
		 }

		 public virtual void AddRecords( string url, ICollection<SrvRecord> records )
		 {
			  records.forEach( r => addRecord( url, r ) );
		 }

		 public virtual void AddRecord( string url, SrvRecord record )
		 {
			 lock ( this )
			 {
				  IList<SrvRecord> srvRecords = _records.getOrDefault( url, new List<SrvRecord>() );
				  srvRecords.Add( record );
      
				  if ( !_records.ContainsKey( url ) )
				  {
						_records[url] = srvRecords;
				  }
			 }
		 }

		 public override Stream<SrvRecord> ResolveSrvRecord( string url )
		 {
			  return Optional.ofNullable( _records[url] ).map( System.Collections.IList.stream ).orElse( Stream.empty() );
		 }
	}

}