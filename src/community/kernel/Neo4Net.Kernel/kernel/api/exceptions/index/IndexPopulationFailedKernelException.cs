﻿using System;

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
namespace Neo4Net.Kernel.Api.Exceptions.index
{
	using KernelException = Neo4Net.Internal.Kernel.Api.exceptions.KernelException;

	public class IndexPopulationFailedKernelException : KernelException
	{
		 private const string FORMAT_MESSAGE = "Failed to populate index %s";

		 public IndexPopulationFailedKernelException( string indexUserDescription, Exception cause ) : base( org.Neo4Net.kernel.api.exceptions.Status_Schema.IndexCreationFailed, cause, FORMAT_MESSAGE, indexUserDescription )
		 {
		 }

		 public IndexPopulationFailedKernelException( string indexUserDescription, string message ) : base( org.Neo4Net.kernel.api.exceptions.Status_Schema.IndexCreationFailed, FORMAT_MESSAGE + ", due to " + message, indexUserDescription )
		 {
		 }
	}

}