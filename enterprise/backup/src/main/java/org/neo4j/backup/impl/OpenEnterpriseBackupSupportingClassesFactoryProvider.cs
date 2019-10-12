/*
 * Copyright (c) 2019 "GraphFoundation" [https://graphfoundation.org]
 *
 * The included source code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html)
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 */

namespace Org.Neo4j.backup.impl
{
	/// 
	public class OpenEnterpriseBackupSupportingClassesFactoryProvider : BackupSupportingClassesFactoryProvider
	{
		 /// 
		 public OpenEnterpriseBackupSupportingClassesFactoryProvider() : base("open-enterprise-backup-support-provider")
		 {
		 }

		 /// <param name="backupModule">
		 /// @return </param>
		 public virtual BackupSupportingClassesFactory GetFactory( BackupModule backupModule )
		 {
			  return new OpenEnterpriseBackupSupportingClassesFactory( backupModule );
		 }

		 /// <summary>
		 /// Make this a higher priority than the one we are extending
		 /// @return
		 /// </summary>
		 protected internal override int Priority
		 {
			 get
			 {
				  return base.Priority + 101;
			 }
		 }
	}


}