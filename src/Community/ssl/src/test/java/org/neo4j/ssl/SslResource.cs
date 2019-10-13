﻿/*
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
namespace Neo4Net.Ssl
{

	public class SslResource
	{
		 private readonly File _privateKey;
		 private readonly File _publicCertificate;
		 private readonly File _trustedDirectory;
		 private readonly File _revokedDirectory;

		 internal SslResource( File privateKey, File publicCertificate, File trustedDirectory, File revokedDirectory )
		 {
			  this._privateKey = privateKey;
			  this._publicCertificate = publicCertificate;
			  this._trustedDirectory = trustedDirectory;
			  this._revokedDirectory = revokedDirectory;
		 }

		 public virtual File PrivateKey()
		 {
			  return _privateKey;
		 }

		 public virtual File PublicCertificate()
		 {
			  return _publicCertificate;
		 }

		 public virtual File TrustedDirectory()
		 {
			  return _trustedDirectory;
		 }

		 public virtual File RevokedDirectory()
		 {
			  return _revokedDirectory;
		 }
	}

}