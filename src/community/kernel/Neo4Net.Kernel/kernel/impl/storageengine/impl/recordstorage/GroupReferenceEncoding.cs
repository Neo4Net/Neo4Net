﻿using System.Diagnostics;

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
namespace Neo4Net.Kernel.impl.storageengine.impl.recordstorage
{
	using References = Neo4Net.Kernel.Impl.Newapi.References;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.kernel.impl.store.record.AbstractBaseRecord.NO_ID;

	internal class GroupReferenceEncoding
	{
		 private const long DIRECT = 0x1000_0000_0000_0000L;

		 /// <summary>
		 /// Encode a relationship id as a group reference.
		 /// </summary>
		 internal static long EncodeRelationship( long relationshipId )
		 {
			  return relationshipId | DIRECT | References.FLAG_MARKER;
		 }

		 /// <summary>
		 /// Check whether a group reference is an encoded relationship id.
		 /// </summary>
		 internal static bool IsRelationship( long groupReference )
		 {
			  Debug.Assert( groupReference != NO_ID );
			  return ( groupReference & References.FLAG_MASK ) == DIRECT;
		 }
	}

}