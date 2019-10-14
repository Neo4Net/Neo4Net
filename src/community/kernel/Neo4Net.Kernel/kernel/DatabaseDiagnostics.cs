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
	using DiagnosticsManager = Neo4Net.Internal.Diagnostics.DiagnosticsManager;
	using DatabaseInfo = Neo4Net.Kernel.impl.factory.DatabaseInfo;
	using KernelDiagnostics = Neo4Net.Kernel.Internal.KernelDiagnostics;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;

	public class DatabaseDiagnostics : LifecycleAdapter
	{
		 private readonly DiagnosticsManager _diagnosticsManager;
		 private readonly NeoStoreDataSource _neoStoreDataSource;
		 private readonly DatabaseInfo _databaseInfo;

		 internal DatabaseDiagnostics( DiagnosticsManager diagnosticsManager, NeoStoreDataSource neoStoreDataSource, DatabaseInfo databaseInfo )
		 {
			  this._diagnosticsManager = diagnosticsManager;
			  this._neoStoreDataSource = neoStoreDataSource;

			  this._databaseInfo = databaseInfo;
		 }
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void start() throws Throwable
		 public override void Start()
		 {
			  _diagnosticsManager.prependProvider( new KernelDiagnostics.Versions( _databaseInfo, _neoStoreDataSource.StoreId ) );
			  _neoStoreDataSource.registerDiagnosticsWith( _diagnosticsManager );
			  _diagnosticsManager.appendProvider( new KernelDiagnostics.StoreFiles( _neoStoreDataSource.DatabaseLayout ) );
		 }
	}

}