using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.coreapi
{

	using Transaction = Neo4Net.GraphDb.Transaction;
	using SecurityContext = Neo4Net.Internal.Kernel.Api.security.SecurityContext;
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