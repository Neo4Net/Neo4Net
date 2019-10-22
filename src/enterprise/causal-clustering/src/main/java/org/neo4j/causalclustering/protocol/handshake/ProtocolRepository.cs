using System;
using System.Collections;
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
namespace Neo4Net.causalclustering.protocol.handshake
{

	using Neo4Net.causalclustering.protocol;
	using Neo4Net.Helpers.Collections;
	using Streams = Neo4Net.Stream.Streams;

	public abstract class ProtocolRepository<U, T> where U : IComparable<U> where T : Neo4Net.causalclustering.protocol.Protocol<U>
	{
		 private readonly IDictionary<Pair<string, U>, T> _protocolMap;
		 private readonly System.Func<string, ISet<U>, ProtocolSelection<U, T>> _protocolSelectionFactory;
		 private System.Func<string, IComparer<T>> _comparator;

		 public ProtocolRepository( T[] protocols, System.Func<string, IComparer<T>> comparators, System.Func<string, ISet<U>, ProtocolSelection<U, T>> protocolSelectionFactory )
		 {
			  this._protocolSelectionFactory = protocolSelectionFactory;
			  IDictionary<Pair<string, U>, T> map = new Dictionary<Pair<string, U>, T>();
			  foreach ( T protocol in protocols )
			  {
					Protocol<U> previous = map[Pair.of( protocol.category(), protocol.implementation() )] = protocol;
					if ( previous != null )
					{
						 throw new System.ArgumentException( string.Format( "Multiple protocols with same identifier and version supplied: {0} and {1}", protocol, previous ) );
					}
			  }
			  _protocolMap = Collections.unmodifiableMap( map );
			  this._comparator = comparators;
		 }

		 internal virtual Optional<T> Select( string protocolName, U version )
		 {
			  return Optional.ofNullable( _protocolMap[Pair.of( protocolName, version )] );
		 }

		 internal virtual Optional<T> Select( string protocolName, ISet<U> versions )
		 {
			  return versions.Select( version => Select( protocolName, version ) ).flatMap( Streams.ofOptional ).Max( _comparator.apply( protocolName ) );
		 }

		 public virtual ProtocolSelection<U, T> GetAll( Neo4Net.causalclustering.protocol.Protocol_Category<T> category, ICollection<U> versions )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  ISet<U> selectedVersions = _protocolMap.SetOfKeyValuePairs().Select(DictionaryEntry.getKey).Where(pair => pair.first().Equals(category.CanonicalName())).Select(Pair::other).Where(version => versions.Count == 0 || versions.Contains(version)).collect(Collectors.toSet());

			  if ( selectedVersions.Count == 0 )
			  {
					throw new System.ArgumentException( string.Format( "Attempted to select protocols for {0} versions {1} but no match in known protocols {2}", category, versions, _protocolMap ) );
			  }
			  else
			  {
					return _protocolSelectionFactory.apply( category.CanonicalName(), selectedVersions );
			  }
		 }

		 internal static IComparer<T> VersionNumberComparator<U, T>() where U : IComparable<U> where T : Neo4Net.causalclustering.protocol.Protocol<U>
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
			  return System.Collections.IComparer.comparing( Protocol::implementation );
		 }
	}

}