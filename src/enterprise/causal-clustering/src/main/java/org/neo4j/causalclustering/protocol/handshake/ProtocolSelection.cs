using System;
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

	public abstract class ProtocolSelection<U, T> where U : IComparable<U> where T : Neo4Net.causalclustering.protocol.Protocol<U>
	{
		 private readonly string _identifier;
		 private readonly ISet<U> _versions;

		 public ProtocolSelection( string identifier, ISet<U> versions )
		 {
			  this._identifier = identifier;
			  this._versions = Collections.unmodifiableSet( versions );
		 }

		 public virtual string Identifier()
		 {
			  return _identifier;
		 }

		 public virtual ISet<U> Versions()
		 {
			  return _versions;
		 }
	}

}