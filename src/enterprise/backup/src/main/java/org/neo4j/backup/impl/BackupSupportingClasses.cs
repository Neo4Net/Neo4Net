using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.backup.impl
{

	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using CloseableResourceManager = Neo4Net.Kernel.Impl.Api.CloseableResourceManager;

	internal class BackupSupportingClasses : AutoCloseable
	{
		 // Strategies
		 private readonly BackupDelegator _backupDelegator;
		 private readonly BackupProtocolService _backupProtocolService;
		 private readonly CloseableResourceManager _closeableResourceManager;

		 // Dependency Helpers
		 private readonly PageCache _pageCache;

		 internal BackupSupportingClasses( BackupDelegator backupDelegator, BackupProtocolService backupProtocolService, PageCache pageCache, ICollection<AutoCloseable> closeables )
		 {
			  this._backupDelegator = backupDelegator;
			  this._backupProtocolService = backupProtocolService;
			  this._pageCache = pageCache;
			  this._closeableResourceManager = new CloseableResourceManager();
			  closeables.forEach( _closeableResourceManager.registerCloseableResource );
		 }

		 public virtual BackupDelegator BackupDelegator
		 {
			 get
			 {
				  return _backupDelegator;
			 }
		 }

		 public virtual BackupProtocolService BackupProtocolService
		 {
			 get
			 {
				  return _backupProtocolService;
			 }
		 }

		 public virtual PageCache PageCache
		 {
			 get
			 {
				  return _pageCache;
			 }
		 }

		 public override void Close()
		 {
			  _closeableResourceManager.closeAllCloseableResources();
		 }
	}

}