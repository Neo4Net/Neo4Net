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
namespace Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.id
{

	using KernelTransactionsSnapshot = Org.Neo4j.Kernel.Impl.Api.KernelTransactionsSnapshot;
	using Lifecycle = Org.Neo4j.Kernel.Lifecycle.Lifecycle;

	/// <summary>
	/// Represent abstraction that responsible for any id related operations on a storage engine level: buffering,
	/// maintenance, clearing, resetting, generation.
	/// </summary>
	public interface IdController : Lifecycle
	{

		 /// <summary>
		 /// Clear underlying id generation infrastructure (clear buffer of ids to reuse, reset buffers, etc.)
		 /// </summary>
		 void Clear();

		 /// <summary>
		 /// Perform ids related maintenance.
		 /// </summary>
		 void Maintenance();

		 void Initialize( System.Func<KernelTransactionsSnapshot> transactionsSnapshotSupplier );
	}

}