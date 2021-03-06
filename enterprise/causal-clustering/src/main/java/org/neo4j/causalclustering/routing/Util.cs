﻿using System.Collections.Generic;

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
namespace Org.Neo4j.causalclustering.routing
{

	using ClientConnector = Org.Neo4j.causalclustering.discovery.ClientConnector;
	using AdvertisedSocketAddress = Org.Neo4j.Helpers.AdvertisedSocketAddress;

	public class Util
	{
		 private Util()
		 {
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public static <T> java.util.List<T> asList(@SuppressWarnings("OptionalUsedAsFieldOrParameterType") java.util.Optional<T> optional)
		 public static IList<T> AsList<T>( Optional<T> optional )
		 {
			  return optional.map( Collections.singletonList ).orElse( emptyList() );
		 }

		 public static System.Func<ClientConnector, AdvertisedSocketAddress> ExtractBoltAddress()
		 {
			  return c => c.connectors().boltAddress();
		 }
	}

}