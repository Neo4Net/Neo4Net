using System;

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
namespace Org.Neo4j.causalclustering.core.consensus.log.segmented
{
	using KernelException = Org.Neo4j.@internal.Kernel.Api.exceptions.KernelException;
	using Status = Org.Neo4j.Kernel.Api.Exceptions.Status;

	// TODO: Should this really be a KernelException?
	public class DamagedLogStorageException : KernelException
	{
		 public DamagedLogStorageException( string format, params object[] args ) : base( org.neo4j.kernel.api.exceptions.Status_General.StorageDamageDetected, format, args )
		 {
		 }

		 public DamagedLogStorageException( Exception cause, string format, params object[] args ) : base( org.neo4j.kernel.api.exceptions.Status_General.StorageDamageDetected, cause, format, args )
		 {
		 }
	}

}