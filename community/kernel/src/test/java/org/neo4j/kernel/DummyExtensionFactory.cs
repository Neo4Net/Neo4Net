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
namespace Org.Neo4j.Kernel
{
	using DatabaseManager = Org.Neo4j.Dbms.database.DatabaseManager;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Org.Neo4j.Kernel.extension;
	using KernelContext = Org.Neo4j.Kernel.impl.spi.KernelContext;
	using KernelData = Org.Neo4j.Kernel.@internal.KernelData;
	using Lifecycle = Org.Neo4j.Kernel.Lifecycle.Lifecycle;

	public class DummyExtensionFactory : KernelExtensionFactory<DummyExtensionFactory.Dependencies>
	{
		 public interface Dependencies
		 {
			  Config Config { get; }

			  KernelData Kernel { get; }

			  DatabaseManager DatabaseManager { get; }
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