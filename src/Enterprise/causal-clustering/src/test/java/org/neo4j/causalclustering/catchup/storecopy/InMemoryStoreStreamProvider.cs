using System.Collections.Generic;
using System.Text;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.causalclustering.catchup.storecopy
{

	public class InMemoryStoreStreamProvider : StoreFileStreamProvider
	{
		 private IDictionary<string, StringBuilder> _fileStreams = new Dictionary<string, StringBuilder>();

		 public override StoreFileStream Acquire( string destination, int requiredAlignment )
		 {
			  if ( !_fileStreams.ContainsKey( destination ) ) _fileStreams.Add( destination, new StringBuilder() );
			  return new InMemoryStoreStream( this, _fileStreams[destination] );
		 }

		 public virtual IDictionary<string, StringBuilder> FileStreams()
		 {
			  return _fileStreams;
		 }

		 internal class InMemoryStoreStream : StoreFileStream
		 {
			 private readonly InMemoryStoreStreamProvider _outerInstance;

			  internal StringBuilder StringBuffer;

			  internal InMemoryStoreStream( InMemoryStoreStreamProvider outerInstance, StringBuilder stringBuffer )
			  {
				  this._outerInstance = outerInstance;
					this.StringBuffer = stringBuffer;
			  }

			  public override void Write( sbyte[] data )
			  {
					foreach ( sbyte b in data )
					{
						 StringBuffer.Append( ( char ) b );
					}
			  }

			  public override void Close()
			  {
					// do nothing
			  }
		 }
	}

}