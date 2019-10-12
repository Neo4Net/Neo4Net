using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.causalclustering.discovery
{

	public class SrvRecordResolverImpl : SrvRecordResolver
	{
		 private readonly string[] _srvRecords = new string[] { "SRV" };
		 private readonly string _srvAttr = "srv";

		 private Optional<InitialDirContext> _idc = null;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.stream.Stream<SrvRecord> resolveSrvRecord(String url) throws javax.naming.NamingException
		 public override Stream<SrvRecord> ResolveSrvRecord( string url )
		 {
			  Attributes attrs = _idc.orElseGet( this.setIdc ).getAttributes( url, _srvRecords );

			  return EnumerationAsStream( ( NamingEnumeration<string> ) attrs.get( _srvAttr ).All ).map( SrvRecord.parse );
		 }

		 private InitialDirContext SetIdc()
		 {
			 lock ( this )
			 {
				  return _idc.orElseGet(() =>
				  {
					Properties env = new Properties();
					env.put( Context.INITIAL_CONTEXT_FACTORY, "com.sun.jndi.dns.DnsContextFactory" );
					try
					{
						 _idc = new InitialDirContext( env );
						 return _idc.get();
					}
					catch ( NamingException e )
					{
						 throw new Exception( e );
					}
				  });
			 }
		 }

		 private static Stream<T> EnumerationAsStream<T>( IEnumerator<T> e )
		 {
			  return StreamSupport.stream(Spliterators.spliteratorUnknownSize(new IteratorAnonymousInnerClass(e)
			 , Spliterator.ORDERED), false);
		 }

		 private class IteratorAnonymousInnerClass : IEnumerator<T>
		 {
			 private IEnumerator<T> _e;

			 public IteratorAnonymousInnerClass( IEnumerator<T> e )
			 {
				 this._e = e;
			 }

			 public T next()
			 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
				  return _e.nextElement();
			 }

			 public bool hasNext()
			 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
				  return _e.hasMoreElements();
			 }
		 }
	}

}