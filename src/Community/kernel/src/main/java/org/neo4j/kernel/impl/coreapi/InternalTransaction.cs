﻿using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Neo4Net.Kernel.impl.coreapi
{

	using Transaction = Neo4Net.Graphdb.Transaction;
	using SecurityContext = Neo4Net.@internal.Kernel.Api.security.SecurityContext;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;

	public interface InternalTransaction : Transaction
	{
		 KernelTransaction.Type TransactionType();

		 SecurityContext SecurityContext();

		 Neo4Net.Kernel.api.KernelTransaction_Revertable OverrideWith( SecurityContext context );

		 Optional<Status> TerminationReason();

		 IDictionary<string, object> MetaData { set; }
	}

}