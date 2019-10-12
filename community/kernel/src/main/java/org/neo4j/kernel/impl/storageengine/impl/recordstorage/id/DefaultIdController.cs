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
	using IdGeneratorFactory = Org.Neo4j.Kernel.impl.store.id.IdGeneratorFactory;
	using LifecycleAdapter = Org.Neo4j.Kernel.Lifecycle.LifecycleAdapter;

	/// <summary>
	/// Default implementation of <seealso cref="IdController"/>.
	/// Do not add any additional possibilities or functionality. Wraps provided <seealso cref="IdGeneratorFactory"/>.
	/// </summary>
	public class DefaultIdController : LifecycleAdapter, IdController
	{
		 public DefaultIdController()
		 {
		 }

		 public override void Clear()
		 {
		 }

		 public override void Maintenance()
		 {
		 }

		 public override void Initialize( System.Func<KernelTransactionsSnapshot> transactionsSnapshotSupplier )
		 {
		 }
	}

}