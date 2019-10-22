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

	public abstract class SrvRecordResolver
	{
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public abstract java.util.stream.Stream<SrvRecord> resolveSrvRecord(String url) throws javax.naming.NamingException;
		 public abstract Stream<SrvRecord> ResolveSrvRecord( string url );

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.stream.Stream<SrvRecord> resolveSrvRecord(String service, String protocol, String hostname) throws javax.naming.NamingException
		 public virtual Stream<SrvRecord> ResolveSrvRecord( string service, string protocol, string hostname )
		 {
			  string url = string.Format( "_{0}._{1}.{2}", service, protocol, hostname );

			  return ResolveSrvRecord( url );
		 }

		 public class SrvRecord
		 {
			  public readonly int Priority;
			  public readonly int Weight;
			  public readonly int Port;
			  public readonly string Host;

			  internal SrvRecord( int priority, int weight, int port, string host )
			  {
					this.Priority = priority;
					this.Weight = weight;
					this.Port = port;
					// Typically the SRV record has a trailing dot - if that is the case we should remove it
					this.Host = host[host.Length - 1] == '.' ? host.Substring( 0, host.Length - 1 ) : host;
			  }

			  public static SrvRecord Parse( string input )
			  {
					string[] parts = input.Split( " ", true );
					return new SrvRecord( int.Parse( parts[0] ), int.Parse( parts[1] ), int.Parse( parts[2] ), parts[3] );
			  }
		 }
	}

}