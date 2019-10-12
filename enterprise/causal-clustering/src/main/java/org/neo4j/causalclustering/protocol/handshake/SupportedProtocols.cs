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
namespace Org.Neo4j.causalclustering.protocol.handshake
{

	using Org.Neo4j.causalclustering.protocol;

	/// <summary>
	/// Keeps track of protocols which are supported by this instance. This is later used when
	/// matching for mutually supported versions during a protocol negotiation.
	/// </summary>
	/// @param <U> Comparable version type. </param>
	/// @param <T> Protocol type. </param>
	public abstract class SupportedProtocols<U, T> where U : IComparable<U> where T : Org.Neo4j.causalclustering.protocol.Protocol<U>
	{
		 private readonly Org.Neo4j.causalclustering.protocol.Protocol_Category<T> _category;
		 private readonly IList<U> _versions;

		 /// <param name="category"> The protocol category. </param>
		 /// <param name="versions"> List of supported versions. An empty list means that every version is supported. </param>
		 internal SupportedProtocols( Org.Neo4j.causalclustering.protocol.Protocol_Category<T> category, IList<U> versions )
		 {
			  this._category = category;
			  this._versions = Collections.unmodifiableList( versions );
		 }

		 public virtual ISet<U> MutuallySupportedVersionsFor( ISet<U> requestedVersions )
		 {
			  if ( Versions().Count == 0 )
			  {
					return requestedVersions;
			  }
			  else
			  {
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
					return requestedVersions.Where( Versions().contains ).collect(Collectors.toSet());
			  }
		 }

		 public virtual Org.Neo4j.causalclustering.protocol.Protocol_Category<T> Identifier()
		 {
			  return _category;
		 }

		 /// <returns> If an empty list then all versions of a matching protocol will be supported </returns>
		 public virtual IList<U> Versions()
		 {
			  return _versions;
		 }
	}

}