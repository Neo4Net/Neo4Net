/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.backup.impl
{

	using OptionalHostnamePort = Neo4Net.Kernel.impl.util.OptionalHostnamePort;

	internal class OnlineBackupRequiredArguments
	{
		 private readonly OptionalHostnamePort _address;
		 private readonly Path _directory;
		 private readonly string _name;
		 private readonly bool _fallbackToFull;
		 private readonly bool _doConsistencyCheck;
		 private readonly long _timeout;
		 private readonly Path _reportDir;
		 private readonly SelectedBackupProtocol _selectedBackupProtocol;

		 internal OnlineBackupRequiredArguments( OptionalHostnamePort address, Path directory, string name, SelectedBackupProtocol selectedBackupProtocol, bool fallbackToFull, bool doConsistencyCheck, long timeout, Path reportDir )
		 {
			  this._address = address;
			  this._directory = directory;
			  this._name = name;
			  this._fallbackToFull = fallbackToFull;
			  this._doConsistencyCheck = doConsistencyCheck;
			  this._timeout = timeout;
			  this._reportDir = reportDir;
			  this._selectedBackupProtocol = selectedBackupProtocol;
		 }

		 public virtual OptionalHostnamePort Address
		 {
			 get
			 {
				  return _address;
			 }
		 }

		 public virtual Path Directory
		 {
			 get
			 {
				  return _directory;
			 }
		 }

		 public virtual string Name
		 {
			 get
			 {
				  return _name;
			 }
		 }

		 public virtual bool FallbackToFull
		 {
			 get
			 {
				  return _fallbackToFull;
			 }
		 }

		 public virtual bool DoConsistencyCheck
		 {
			 get
			 {
				  return _doConsistencyCheck;
			 }
		 }

		 public virtual long Timeout
		 {
			 get
			 {
				  return _timeout;
			 }
		 }

		 public virtual Path ReportDir
		 {
			 get
			 {
				  return _reportDir;
			 }
		 }

		 public virtual Path ResolvedLocationFromName
		 {
			 get
			 {
				  return _directory.resolve( _name );
			 }
		 }

		 public virtual SelectedBackupProtocol SelectedBackupProtocol
		 {
			 get
			 {
				  return _selectedBackupProtocol;
			 }
		 }
	}

}