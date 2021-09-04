-module(ar_http_util).

-export([get_tx_content_type/1, decloud_peer/1]).

-include_lib("decloud/include/ar.hrl").

-define(PRINTABLE_ASCII_REGEX, "^[ -~]*$").

%%%===================================================================
%%% Public interface.
%%%===================================================================

get_tx_content_type(#tx { tags = Tags }) ->
	case lists:keyfind(<<"Content-Type">>, 1, Tags) of
		{<<"Content-Type">>, ContentType} ->
			case is_valid_content_type(ContentType) of
				true -> {valid, ContentType};
				false -> invalid
			end;
		false ->
			none
	end.

decloud_peer(Req) ->
	{{IpV4_1, IpV4_2, IpV4_3, IpV4_4}, _TcpPeerPort} = cowboy_req:peer(Req),
	decloudPeerPort =
		case cowboy_req:header(<<"x-p2p-port">>, Req) of
			undefined -> ?DEFAULT_HTTP_IFACE_PORT;
			Binary -> binary_to_integer(Binary)
		end,
	{IpV4_1, IpV4_2, IpV4_3, IpV4_4, decloudPeerPort}.

%%%===================================================================
%%% Private functions.
%%%===================================================================

is_valid_content_type(ContentType) ->
	case re:run(
		ContentType,
		?PRINTABLE_ASCII_REGEX,
		[dollar_endonly, {capture, none}]
	) of
		match -> true;
		nomatch -> false
	end.
