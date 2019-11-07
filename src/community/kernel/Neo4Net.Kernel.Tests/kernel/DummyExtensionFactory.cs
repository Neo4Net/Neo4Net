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
namespace Neo4Net.Kernel
{
	using IDatabaseManager = Neo4Net.Dbms.database.DatabaseManager;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Neo4Net.Kernel.extension;
	using KernelContext = Neo4Net.Kernel.impl.spi.KernelContext;
	using KernelData = Neo4Net.Kernel.Internal.KernelData;
	using Lifecycle = Neo4Net.Kernel.Lifecycle.Lifecycle;

	public class DummyExtensionFactory : KernelExtensionFactory<DummyExtensionFactory.Dependencies>
	{
		 public interface Dependencies
		 {
			  Config Config { get; }

			  KernelData Kernel { get; }

			  IDatabaseManager IDatabaseManager { get; }
		 }

		 internal const string EXTENSION_ID = "dummy";

		 public DummyExtensionFactory() : base(EXTENSION_ID)
		 {
		 }

		 public override Lifecycle NewInstance( KernelContext context, Dependencies dependencies )
		 {
			  return new DummyExtension( dependencies );
		 }
	}

}