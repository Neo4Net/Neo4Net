using System;

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
namespace Neo4Net.Kernel.impl.spi
{

	using DatabaseInfo = Neo4Net.Kernel.impl.factory.DatabaseInfo;
	using DependencySatisfier = Neo4Net.Kernel.impl.util.DependencySatisfier;

	public class SimpleKernelContext : KernelContext
	{
		 private readonly File _contextDirectory;
		 private readonly DatabaseInfo _databaseInfo;
		 private readonly DependencySatisfier _satisfier;

		 public SimpleKernelContext( File contextDirectory, DatabaseInfo databaseInfo, DependencySatisfier satisfier )
		 {
			  this._contextDirectory = contextDirectory;
			  this._databaseInfo = databaseInfo;
			  this._satisfier = satisfier;
		 }

		 [Obsolete]
		 public override File StoreDir()
		 {
			  return _contextDirectory;
		 }

		 public override DatabaseInfo DatabaseInfo()
		 {
			  return _databaseInfo;
		 }

		 public override DependencySatisfier DependencySatisfier()
		 {
			  return _satisfier;
		 }
	}

}