using System;
using System.Collections.Generic;

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
namespace Neo4Net.Server.rest.security
{

	public class SecurityFilter : Filter
	{
		 private readonly Dictionary<UriPathWildcardMatcher, HashSet<ForbiddingSecurityRule>> _rules = new Dictionary<UriPathWildcardMatcher, HashSet<ForbiddingSecurityRule>>();

		 public SecurityFilter( SecurityRule rule, params SecurityRule[] rules ) : this( Merge( rule, rules ) )
		 {
		 }

		 public SecurityFilter( IEnumerable<SecurityRule> securityRules )
		 {
			  // For backwards compatibility
			  foreach ( SecurityRule r in securityRules )
			  {
					string rulePath = r.ForUriPath();
					if ( !rulePath.EndsWith( "*", StringComparison.Ordinal ) )
					{
						 rulePath = rulePath + "*";
					}

					UriPathWildcardMatcher uriPathWildcardMatcher = new UriPathWildcardMatcher( rulePath );
					HashSet<ForbiddingSecurityRule> ruleHashSet = _rules.computeIfAbsent( uriPathWildcardMatcher, k => new HashSet<ForbiddingSecurityRule>() );
					ruleHashSet.Add( FromSecurityRule( r ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private static ForbiddingSecurityRule fromSecurityRule(final SecurityRule rule)
		 private static ForbiddingSecurityRule FromSecurityRule( SecurityRule rule )
		 {
			  if ( rule is ForbiddingSecurityRule )
			  {
					return ( ForbiddingSecurityRule ) rule;
			  }
			  return new ForbiddenRuleDecorator( rule );
		 }

		 private static IEnumerable<SecurityRule> Merge( SecurityRule rule, SecurityRule[] rules )
		 {
			  List<SecurityRule> result = new List<SecurityRule>();

			  result.Add( rule );

			  Collections.addAll( result, rules );

			  return result;
		 }

		 public static string BasicAuthenticationResponse( string realm )
		 {
			  return "Basic realm=\"" + realm + "\"";
		 }

		 public override void Init( FilterConfig filterConfig )
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void doFilter(javax.servlet.ServletRequest request, javax.servlet.ServletResponse response, javax.servlet.FilterChain chain) throws java.io.IOException, javax.servlet.ServletException
		 public override void DoFilter( ServletRequest request, ServletResponse response, FilterChain chain )
		 {

			  ValidateRequestType( request );
			  ValidateResponseType( response );

			  HttpServletRequest httpReq = ( HttpServletRequest ) request;
			  string path = httpReq.ContextPath + ( httpReq.PathInfo == null ? "" : httpReq.PathInfo );

			  bool requestIsForbidden = false;
			  foreach ( UriPathWildcardMatcher uriPathWildcardMatcher in _rules.Keys )
			  {
					if ( uriPathWildcardMatcher.Matches( path ) )
					{
						 HashSet<ForbiddingSecurityRule> securityRules = _rules[uriPathWildcardMatcher];
						 foreach ( ForbiddingSecurityRule securityRule in securityRules )
						 {
							  // 401 on the first failed rule we come along
							  if ( !securityRule.IsAuthorized( httpReq ) )
							  {
									CreateUnauthorizedChallenge( response, securityRule );
									return;
							  }
							  requestIsForbidden |= securityRule.IsForbidden( httpReq );
						 }
					}
			  }
			  if ( requestIsForbidden )
			  {
					CreateForbiddenResponse( response );
					return;
			  }

			  chain.doFilter( request, response );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void validateRequestType(javax.servlet.ServletRequest request) throws javax.servlet.ServletException
		 private void ValidateRequestType( ServletRequest request )
		 {
			  if ( !( request is HttpServletRequest ) )
			  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getCanonicalName method:
					throw new ServletException( string.Format( "Expected HttpServletRequest, received [{0}]", request.GetType().FullName ) );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void validateResponseType(javax.servlet.ServletResponse response) throws javax.servlet.ServletException
		 private void ValidateResponseType( ServletResponse response )
		 {
			  if ( !( response is HttpServletResponse ) )
			  {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getCanonicalName method:
					throw new ServletException( string.Format( "Expected HttpServletResponse, received [{0}]", response.GetType().FullName ) );
			  }
		 }

		 private void CreateUnauthorizedChallenge( ServletResponse response, SecurityRule rule )
		 {
			  HttpServletResponse httpServletResponse = ( HttpServletResponse ) response;
			  httpServletResponse.Status = 401;
			  httpServletResponse.addHeader( "WWW-Authenticate", rule.WwwAuthenticateHeader() );
		 }

		 private void CreateForbiddenResponse( ServletResponse response )
		 {
			  HttpServletResponse httpServletResponse = ( HttpServletResponse ) response;
			  httpServletResponse.Status = 403;
		 }

		 public override void Destroy()
		 {
			 lock ( this )
			 {
				  _rules.Clear();
			 }
		 }

		 private class ForbiddenRuleDecorator : ForbiddingSecurityRule
		 {
			  internal readonly SecurityRule InnerRule;

			  internal ForbiddenRuleDecorator( SecurityRule rule )
			  {
					this.InnerRule = rule;
			  }

			  public override bool IsForbidden( HttpServletRequest request )
			  {
					return false;
			  }

			  public override bool IsAuthorized( HttpServletRequest request )
			  {
					return InnerRule.isAuthorized( request );
			  }

			  public override string ForUriPath()
			  {
					return InnerRule.forUriPath();
			  }

			  public override string WwwAuthenticateHeader()
			  {
					return InnerRule.wwwAuthenticateHeader();
			  }
		 }
	}

}